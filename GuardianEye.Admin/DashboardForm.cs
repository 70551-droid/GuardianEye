using GuardianEye.Shared;

namespace GuardianEye.Admin;

public partial class DashboardForm : Form
{
    private readonly TcpServer _tcpServer;
    private readonly UdpBroadcaster _udpBroadcaster;
    private readonly SessionManager _sessionManager;
    private readonly DatabaseService _database;
    private readonly System.Windows.Forms.Timer _refreshTimer;

    public DashboardForm(TcpServer tcpServer, UdpBroadcaster udpBroadcaster, SessionManager sessionManager, DatabaseService database)
    {
        InitializeComponent();
        _sessionManager = sessionManager;
        _database = database;

        _tcpServer.TimeRequestReceived += OnTimeRequest;
        _tcpServer.ClientConnected += OnClientConnected;
        _tcpServer.ClientDisconnected += OnClientDisconnected;
        dataGridViewClients.CellFormatting += DataGridViewClients_CellFormatting;

        _refreshTimer = new System.Windows.Forms.Timer { Interval = 2000 };
        _refreshTimer.Tick += (_, _) => RefreshClientList();
        _refreshTimer.Start();

        _udpBroadcaster.Start();
        _tcpServer.Start();

        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
            UIStyles.EnableRoundedCorners(Handle);
        };
    }

    private void buttonFilters_Click(object sender, EventArgs e)
    {
        using var form = new FilterConfigForm(_sessionManager);
        form.ShowDialog(this);
    }

    private void buttonReports_Click(object sender, EventArgs e)
    {
        using var form = new ReportForm(_database);
        form.ShowDialog(this);
    }

    private void buttonGroups_Click(object sender, EventArgs e)
    {
        using var form = new GroupManagementForm(_database);
        form.ShowDialog(this);
    }

    private void buttonExportPolicy_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "JSON files (*.json)|*.json",
            FileName = $"GuardianEye_Policy_{DateTime.Now:yyyyMMdd}.json"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            PolicyManager.ExportPolicy(sfd.FileName, _database);
            MessageBox.Show($"Policy exported to {sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void buttonImportPolicy_Click(object sender, EventArgs e)
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "JSON files (*.json)|*.json"
        };
        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (MessageBox.Show("Import will overwrite existing groups and student configurations. Continue?",
                "Confirm Import", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                PolicyManager.ImportPolicy(ofd.FileName, _database);
                MessageBox.Show("Policy imported successfully.", "Import Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

    private void buttonSendChat_Click(object sender, EventArgs e)
    {
        string text = textBoxChatMessage.Text.Trim();
        if (string.IsNullOrEmpty(text)) return;

        string target = comboBoxChatTarget.SelectedItem as string;
        if (string.IsNullOrEmpty(target)) target = "All Students";

        var msg = new ChatMessage
        {
            Sender = "Admin",
            Message = text,
            Timestamp = DateTime.UtcNow,
            IsBroadcast = target == "All Students"
        };

        if (msg.IsBroadcast)
        {
            _sessionManager.BroadcastMessage(msg);
            listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Admin → All: \"{text}\"");
        }
        else
        {
            _sessionManager.SendMessage(target, msg);
            listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Admin → {target}: \"{text}\"");
        }

        textBoxChatMessage.Clear();
    }

    private void OnClientConnected(object sender, ClientInfo client)
    {
        BeginInvoke(() =>
        {
            listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {client.DisplayName} connected ({client.RemainingTimeSeconds / 60} min)");
            UpdateStatusBar();
            UpdateChatTargets();
        });
    }

    private void OnClientDisconnected(object sender, string username)
    {
        BeginInvoke(() =>
        {
            listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] {username} disconnected");
            UpdateStatusBar();
            UpdateChatTargets();
        });
    }

    private void OnTimeRequest(object sender, TimeRequestMessage request)
    {
        BeginInvoke(() =>
        {
            var client = _sessionManager.GetClientBySessionId(request.SessionId);
            string name = client?.DisplayName ?? request.SessionId.ToString();
            listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Time request from {name}: +{request.RequestedMinutes} min (\"{request.Reason}\")");

            var result = MessageBox.Show(
                $"{name} requests {request.RequestedMinutes} more minutes.\nReason: {request.Reason}\n\nApprove?",
                "Time Request",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result == DialogResult.Yes && client != null)
            {
                _sessionManager.SendMessage(client.Username, new AdminCommandMessage
                {
                    Command = AdminCommandType.AddTime,
                    MinutesToAdd = request.RequestedMinutes
                });
                listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Approved {request.RequestedMinutes} min for {name}");
            }
        });
    }

    private void RefreshClientList()
    {
        var clients = _sessionManager.Clients;
        dataGridViewClients.Rows.Clear();
        foreach (var client in clients)
        {
            int min = client.RemainingTimeSeconds / 60;
            int sec = client.RemainingTimeSeconds % 60;
            string currentApp = FormatCurrentApp(client.CurrentProcess, client.CurrentWindow);
            dataGridViewClients.Rows.Add(
                client.DisplayName,
                client.Username,
                client.GroupName,
                $"{min:D2}:{sec:D2}",
                client.Status.ToString(),
                currentApp,
                "Add Time",
                "Force Logout"
            );
        }
        UpdateStatusBar();
    }

    private static string FormatCurrentApp(string process, string title)
    {
        if (string.IsNullOrEmpty(process)) return "-";
        if (string.IsNullOrEmpty(title)) return process;

        string browserSuffix = process.ToLower() switch
        {
            "chrome" => " - Google Chrome",
            "msedge" => " - Microsoft Edge",
            "firefox" => " - Mozilla Firefox",
            "opera" => " - Opera",
            "brave" => " - Brave",
            "iexplore" => " - Internet Explorer",
            _ => null
        };

        if (browserSuffix != null && title.EndsWith(browserSuffix, StringComparison.OrdinalIgnoreCase))
        {
            string page = title.Substring(0, title.Length - browserSuffix.Length);
            if (page.Length > 40) page = page[..37] + "...";
            return page;
        }

        string full = $"{process} — {title}";
        return full.Length > 50 ? full[..47] + "..." : full;
    }

    private void UpdateStatusBar()
    {
        int count = _sessionManager.Clients.Count;
        int active = _sessionManager.Clients.Count(c => c.Status == ClientStatusType.Active);
        int totalToday = 0;
        try { var summary = _database.GetTodaySummary(); totalToday = summary.TotalMinutes; } catch { }

        labelStatus.Text = $"Connected clients: {count}";
        labelStatStudentsVal.Text = count.ToString();
        labelStatActiveVal.Text = active.ToString();
        labelStatMinutesVal.Text = totalToday.ToString();
    }

    private void DataGridViewClients_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex == 4 && e.Value != null)
        {
            string status = e.Value.ToString();
            e.CellStyle.ForeColor = status switch
            {
                "Active" => UIStyles.AccentGreen,
                "Idle" => UIStyles.AccentAmber,
                "Offline" => UIStyles.AccentRed,
                _ => UIStyles.TextPrimary
            };
            e.CellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        }
    }

    private void UpdateChatTargets()
    {
        comboBoxChatTarget.Items.Clear();
        comboBoxChatTarget.Items.Add("All Students");
        foreach (var c in _sessionManager.Clients)
            comboBoxChatTarget.Items.Add(c.Username);
        if (comboBoxChatTarget.SelectedIndex < 0)
            comboBoxChatTarget.SelectedIndex = 0;
    }

    private void dataGridViewClients_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0) return;
        string username = dataGridViewClients.Rows[e.RowIndex].Cells[1].Value as string;
        if (username == null) return;

        if (e.ColumnIndex == 6) // Add Time
        {
            string input = PromptForInput("Minutes to add:", "Add Time", "5");
            if (int.TryParse(input, out int mins) && mins > 0)
            {
                _sessionManager.SendMessage(username, new AdminCommandMessage
                {
                    Command = AdminCommandType.AddTime,
                    MinutesToAdd = mins
                });
                listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Manually added {mins} min for {username}");
            }
        }
        else if (e.ColumnIndex == 7) // Force Logout
        {
            var result = MessageBox.Show($"Force logout {username}?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                _sessionManager.SendMessage(username, new AdminCommandMessage
                {
                    Command = AdminCommandType.ForceLogout
                });
                var client = _sessionManager.GetClient(username);
                if (client != null)
                {
                    _database.EndSession(client.SessionId);
                    _sessionManager.RemoveClient(username);
                }
                listBoxLog.Items.Insert(0, $"[{DateTime.Now:HH:mm:ss}] Force logged out {username}");
            }
        }
    }

    private static string PromptForInput(string prompt, string title, string defaultValue)
    {
        var form = new Form
        {
            Width = 380, Height = 200,
            FormBorderStyle = FormBorderStyle.None,
            StartPosition = FormStartPosition.CenterParent,
            MinimizeBox = false, MaximizeBox = false,
            Text = title,
            BackColor = UIStyles.DeepBg
        };
        var panel = new Guna.UI2.WinForms.Guna2Panel
        {
            FillColor = UIStyles.GlassCard,
            BorderColor = UIStyles.GlassBorder,
            BorderRadius = 12,
            BorderThickness = 1,
            Size = new Size(360, 160),
            Location = new Point(10, 10)
        };
        var label = new Label { Left = 16, Top = 16, Width = 320, Text = prompt, ForeColor = UIStyles.TextPrimary, BackColor = Color.Transparent };
        var textBox = new Guna.UI2.WinForms.Guna2TextBox
        {
            Location = new Point(16, 44), Size = new Size(320, 36),
            Text = defaultValue, ForeColor = UIStyles.TextPrimary,
            FillColor = Color.FromArgb(40, 255, 255, 255),
            BorderColor = Color.FromArgb(50, 255, 255, 255),
            BorderRadius = 8
        };
        textBox.FocusedState.BorderColor = UIStyles.AccentBlue;
        var okBtn = new Guna.UI2.WinForms.Guna2Button
        {
            Text = "OK", Location = new Point(160, 96), Size = new Size(80, 34),
            DialogResult = DialogResult.OK
        };
        UIStyles.StyleGunaButton(okBtn, UIStyles.AccentBlue);
        var cancelBtn = new Guna.UI2.WinForms.Guna2Button
        {
            Text = "Cancel", Location = new Point(250, 96), Size = new Size(80, 34),
            DialogResult = DialogResult.Cancel
        };
        UIStyles.StyleGunaButton(cancelBtn, Color.FromArgb(80, 80, 100));
        panel.Controls.AddRange(new Control[] { label, textBox, okBtn, cancelBtn });
        form.Controls.Add(panel);
        form.AcceptButton = okBtn;
        form.CancelButton = cancelBtn;
        form.Load += (_, _) => { if (Environment.OSVersion.Version.Build >= 22000) UIStyles.EnableAcrylic(form.Handle); };
        return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _refreshTimer.Stop();
        _tcpServer.Stop();
        _udpBroadcaster.Stop();
        _sessionManager.Dispose();
        base.OnFormClosing(e);
    }
}
