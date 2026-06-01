namespace GuardianEye.Admin;

partial class DashboardForm
{
    private System.ComponentModel.IContainer components = null;
    private Guna.UI2.WinForms.Guna2DataGridView dataGridViewClients;
    private ListBox listBoxLog;
    private Guna.UI2.WinForms.Guna2Panel panelStatStudents;
    private Guna.UI2.WinForms.Guna2Panel panelStatActive;
    private Guna.UI2.WinForms.Guna2Panel panelStatMinutes;
    private Label labelStatStudents;
    private Label labelStatActive;
    private Label labelStatMinutes;
    private Label labelStatStudentsVal;
    private Label labelStatActiveVal;
    private Label labelStatMinutesVal;
    private Guna.UI2.WinForms.Guna2Button buttonFilters;
    private Guna.UI2.WinForms.Guna2Button buttonReports;
    private Guna.UI2.WinForms.Guna2Button buttonGroups;
    private Guna.UI2.WinForms.Guna2Button buttonExportPolicy;
    private Guna.UI2.WinForms.Guna2Button buttonImportPolicy;
    private Guna.UI2.WinForms.Guna2ComboBox comboBoxChatTarget;
    private Guna.UI2.WinForms.Guna2TextBox textBoxChatMessage;
    private Guna.UI2.WinForms.Guna2Button buttonSendChat;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel labelStatus;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        dataGridViewClients = new Guna.UI2.WinForms.Guna2DataGridView();
        listBoxLog = new ListBox();
        panelStatStudents = new Guna.UI2.WinForms.Guna2Panel();
        panelStatActive = new Guna.UI2.WinForms.Guna2Panel();
        panelStatMinutes = new Guna.UI2.WinForms.Guna2Panel();
        labelStatStudentsVal = new Label();
        labelStatActiveVal = new Label();
        labelStatMinutesVal = new Label();
        labelStatStudents = new Label();
        labelStatActive = new Label();
        labelStatMinutes = new Label();
        buttonFilters = new Guna.UI2.WinForms.Guna2Button();
        buttonReports = new Guna.UI2.WinForms.Guna2Button();
        buttonGroups = new Guna.UI2.WinForms.Guna2Button();
        buttonExportPolicy = new Guna.UI2.WinForms.Guna2Button();
        buttonImportPolicy = new Guna.UI2.WinForms.Guna2Button();
        comboBoxChatTarget = new Guna.UI2.WinForms.Guna2ComboBox();
        textBoxChatMessage = new Guna.UI2.WinForms.Guna2TextBox();
        buttonSendChat = new Guna.UI2.WinForms.Guna2Button();
        statusStrip = new StatusStrip();
        labelStatus = new ToolStripStatusLabel();

        ((System.ComponentModel.ISupportInitialize)dataGridViewClients).BeginInit();
        SuspendLayout();

        // Stat Cards Row
        int statCardY = 12;
        int statCardH = 70;
        int statCardW = 220;

        void SetupStatCard(Guna.UI2.WinForms.Guna2Panel panel, int x, string label, string val, Label labelCtl, Label valCtl, Color accent)
        {
            panel.BackColor = Color.Transparent;
            panel.FillColor = UIStyles.GlassCard;
            panel.BorderColor = UIStyles.GlassBorder;
            panel.BorderRadius = 12;
            panel.BorderThickness = 1;
            panel.Size = new Size(statCardW, statCardH);
            panel.Location = new Point(x, statCardY);

            var accentBar = new Guna.UI2.WinForms.Guna2Panel();
            accentBar.Size = new Size(4, statCardH - 20);
            accentBar.Location = new Point(12, 10);
            accentBar.FillColor = accent;
            accentBar.BorderRadius = 2;
            panel.Controls.Add(accentBar);

            valCtl.AutoSize = false;
            valCtl.Size = new Size(100, 28);
            valCtl.Location = new Point(24, 8);
            valCtl.Text = val;
            valCtl.Font = new Font("Segoe UI", 20F, FontStyle.Bold);
            valCtl.ForeColor = UIStyles.TextPrimary;
            valCtl.BackColor = Color.Transparent;
            panel.Controls.Add(valCtl);

            labelCtl.AutoSize = false;
            labelCtl.Size = new Size(180, 18);
            labelCtl.Location = new Point(24, 42);
            labelCtl.Text = label;
            labelCtl.Font = new Font("Segoe UI", 10F);
            labelCtl.ForeColor = UIStyles.TextMuted;
            labelCtl.BackColor = Color.Transparent;
            panel.Controls.Add(labelCtl);
        }

        SetupStatCard(panelStatStudents, 12, "Total Students", "0", labelStatStudents, labelStatStudentsVal, UIStyles.AccentBlue);
        SetupStatCard(panelStatActive, 240, "Active Now", "0", labelStatActive, labelStatActiveVal, UIStyles.AccentGreen);
        SetupStatCard(panelStatMinutes, 468, "Today's Minutes", "0", labelStatMinutes, labelStatMinutesVal, UIStyles.AccentPurple);

        // DataGridView
        dataGridViewClients.AllowUserToAddRows = false;
        dataGridViewClients.AllowUserToDeleteRows = false;
        dataGridViewClients.BackgroundColor = UIStyles.GlassCard;
        dataGridViewClients.ColumnHeadersHeight = 32;
        dataGridViewClients.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        dataGridViewClients.Columns.Add("Name", "Student");
        dataGridViewClients.Columns.Add("Username", "Username");
        dataGridViewClients.Columns.Add("Group", "Group");
        dataGridViewClients.Columns.Add("Time", "Remaining");
        dataGridViewClients.Columns.Add("Status", "Status");
        dataGridViewClients.Columns.Add("CurrentApp", "Current App");
        dataGridViewClients.Columns.Add("AddTime", "Add Time");
        dataGridViewClients.Columns.Add("Logout", "Force Logout");
        dataGridViewClients.Location = new Point(12, 92);
        dataGridViewClients.Size = new Size(876, 200);
        dataGridViewClients.TabIndex = 0;
        dataGridViewClients.CellClick += dataGridViewClients_CellClick;
        dataGridViewClients.BorderStyle = BorderStyle.None;
        dataGridViewClients.RowHeadersVisible = false;
        dataGridViewClients.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dataGridViewClients.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dataGridViewClients.GridColor = Color.FromArgb(50, 255, 255, 255);
        dataGridViewClients.ForeColor = UIStyles.TextPrimary;
        dataGridViewClients.RowTemplate.Height = 28;
        dataGridViewClients.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dataGridViewClients.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dataGridViewClients.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 60);
        dataGridViewClients.ColumnHeadersDefaultCellStyle.ForeColor = UIStyles.TextPrimary;
        dataGridViewClients.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);

        // Activity Log
        listBoxLog.BackColor = Color.FromArgb(25, 25, 45);
        listBoxLog.ForeColor = UIStyles.TextPrimary;
        listBoxLog.Font = new Font("Consolas", 9F);
        listBoxLog.FormattingEnabled = true;
        listBoxLog.ItemHeight = 14;
        listBoxLog.Location = new Point(12, 300);
        listBoxLog.Size = new Size(876, 116);
        listBoxLog.TabIndex = 1;
        listBoxLog.BorderStyle = BorderStyle.None;

        // Action Buttons Row
        int btnY = 425;
        int btnW = 110;
        int btnH = 34;

        void MakeBtn(Guna.UI2.WinForms.Guna2Button btn, int x, string text, Color accent, EventHandler click)
        {
            btn.Size = new Size(btnW, btnH);
            btn.Location = new Point(x, btnY);
            btn.Text = text;
            UIStyles.StyleGunaButton(btn, accent);
            btn.Click += click;
        }

        MakeBtn(buttonFilters, 12, "Filters", UIStyles.AccentPurple, buttonFilters_Click);
        MakeBtn(buttonReports, 130, "Reports", UIStyles.AccentBlue, buttonReports_Click);
        MakeBtn(buttonGroups, 248, "Groups", UIStyles.AccentGreen, buttonGroups_Click);
        MakeBtn(buttonExportPolicy, 366, "Export", Color.FromArgb(160, 120, 200), buttonExportPolicy_Click);
        MakeBtn(buttonImportPolicy, 484, "Import", UIStyles.AccentAmber, buttonImportPolicy_Click);

        // Chat Section
        comboBoxChatTarget.FillColor = Color.FromArgb(40, 255, 255, 255);
        comboBoxChatTarget.BorderColor = Color.FromArgb(50, 255, 255, 255);
        comboBoxChatTarget.BorderRadius = 8;
        comboBoxChatTarget.ForeColor = UIStyles.TextPrimary;
        comboBoxChatTarget.Items.Add("All Students");
        comboBoxChatTarget.Location = new Point(12, 470);
        comboBoxChatTarget.Size = new Size(140, 30);
        comboBoxChatTarget.SelectedIndex = 0;
        comboBoxChatTarget.DropDownStyle = ComboBoxStyle.DropDownList;

        textBoxChatMessage.FillColor = Color.FromArgb(40, 255, 255, 255);
        textBoxChatMessage.BorderColor = Color.FromArgb(50, 255, 255, 255);
        textBoxChatMessage.FocusedState.BorderColor = UIStyles.AccentBlue;
        textBoxChatMessage.BorderRadius = 8;
        textBoxChatMessage.PlaceholderText = "Type a chat message...";
        textBoxChatMessage.ForeColor = UIStyles.TextPrimary;
        textBoxChatMessage.PlaceholderForeColor = UIStyles.TextMuted;
        textBoxChatMessage.Location = new Point(160, 470);
        textBoxChatMessage.Size = new Size(600, 30);

        buttonSendChat.Size = new Size(100, 30);
        buttonSendChat.Location = new Point(770, 470);
        buttonSendChat.Text = "Send";
        UIStyles.StyleGunaButton(buttonSendChat, UIStyles.AccentBlue);
        buttonSendChat.Click += buttonSendChat_Click;

        // Status Strip
        statusStrip.Items.Add(labelStatus);
        statusStrip.BackColor = Color.FromArgb(16, 16, 36);
        statusStrip.ForeColor = UIStyles.TextMuted;
        labelStatus.Text = "Connected clients: 0";

        // Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(900, 520);
        Controls.AddRange(new Control[] {
            panelStatStudents, panelStatActive, panelStatMinutes,
            dataGridViewClients, listBoxLog,
            buttonFilters, buttonReports, buttonGroups,
            buttonExportPolicy, buttonImportPolicy,
            comboBoxChatTarget, textBoxChatMessage, buttonSendChat,
            statusStrip
        });
        FormBorderStyle = FormBorderStyle.FixedSingle;
        Name = "DashboardForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GuardianEye - Admin Dashboard";
        ((System.ComponentModel.ISupportInitialize)dataGridViewClients).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
