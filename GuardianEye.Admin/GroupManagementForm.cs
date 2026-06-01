namespace GuardianEye.Admin;

public partial class GroupManagementForm : Form
{
    private readonly DatabaseService _db;

    public GroupManagementForm(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
            UIStyles.EnableRoundedCorners(Handle);
        };
        LoadGroups();
        LoadStudents();
    }

    private void LoadGroups()
    {
        dataGridViewGroups.Rows.Clear();
        foreach (var g in _db.GetAllGroups())
        {
            int studentCount = _db.GetAllStudents().Count(s => s.GroupId == g.Id);
            dataGridViewGroups.Rows.Add(g.Id, g.Name, g.DailyLimit / 60, studentCount);
        }
    }

    private void LoadStudents()
    {
        listBoxStudents.Items.Clear();
        var groups = _db.GetAllGroups();
        foreach (var s in _db.GetAllStudents())
        {
            string groupInfo = string.IsNullOrEmpty(s.GroupName) ? "No Group" : s.GroupName;
            listBoxStudents.Items.Add($"{s.Username} | {s.DisplayName} | Limit: {s.DailyLimit / 60}h | Group: {groupInfo}");
        }
    }

    private void buttonAddGroup_Click(object sender, EventArgs e)
    {
        string name = PromptForInput("Group name:", "Add Group", "");
        if (string.IsNullOrWhiteSpace(name)) return;
        string limitStr = PromptForInput("Daily limit (hours):", "Add Group", "1");
        if (!int.TryParse(limitStr, out int hours) || hours < 1) return;

        _db.CreateGroup(name.Trim(), hours * 3600);
        LoadGroups();
    }

    private void buttonEditGroup_Click(object sender, EventArgs e)
    {
        if (dataGridViewGroups.CurrentRow == null) return;
        int id = (int)dataGridViewGroups.CurrentRow.Cells[0].Value;
        string name = dataGridViewGroups.CurrentRow.Cells[1].Value as string;
        int currentHours = (int)dataGridViewGroups.CurrentRow.Cells[2].Value;

        string newName = PromptForInput("Group name:", "Edit Group", name);
        if (string.IsNullOrWhiteSpace(newName)) return;
        string limitStr = PromptForInput("Daily limit (hours):", "Edit Group", currentHours.ToString());
        if (!int.TryParse(limitStr, out int hours) || hours < 1) return;

        _db.UpdateGroup(id, newName.Trim(), hours * 3600);
        LoadGroups();
    }

    private void buttonDeleteGroup_Click(object sender, EventArgs e)
    {
        if (dataGridViewGroups.CurrentRow == null) return;
        int id = (int)dataGridViewGroups.CurrentRow.Cells[0].Value;
        string name = dataGridViewGroups.CurrentRow.Cells[1].Value as string;

        if (name == "Default")
        {
            MessageBox.Show("Cannot delete the Default group.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (MessageBox.Show($"Delete group '{name}'? Students will become ungrouped.", "Confirm",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
        {
            _db.DeleteGroup(id);
            LoadGroups();
            LoadStudents();
        }
    }

    private void buttonEditStudent_Click(object sender, EventArgs e)
    {
        int selectedIndex = listBoxStudents.SelectedIndex;
        if (selectedIndex < 0) return;

        var students = _db.GetAllStudents();
        if (selectedIndex >= students.Count) return;

        var student = students[selectedIndex];

        string newName = PromptForInput("Display name:", "Edit Student", student.DisplayName);
        if (string.IsNullOrWhiteSpace(newName)) return;
        string limitStr = PromptForInput("Daily limit (hours):", "Edit Student", (student.DailyLimit / 60).ToString());
        if (!int.TryParse(limitStr, out int hours) || hours < 1) return;

        var groups = _db.GetAllGroups();
        var groupNames = groups.Select(g => g.Name).ToArray();
        string selectedGroup = PromptForChoice("Select group:", "Edit Student", groupNames,
            student.GroupName ?? "Default");

        int? groupId = null;
        if (!string.IsNullOrEmpty(selectedGroup))
        {
            var match = groups.FirstOrDefault(g => g.Name == selectedGroup);
            if (match.Id > 0) groupId = match.Id;
        }

        _db.UpdateStudent(student.Id, newName.Trim(), hours * 3600, groupId);
        LoadStudents();
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

    private static string PromptForChoice(string prompt, string title, string[] options, string defaultOption)
    {
        var form = new Form
        {
            Width = 380, Height = 220,
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
            Size = new Size(360, 180),
            Location = new Point(10, 10)
        };
        var label = new Label { Left = 16, Top = 16, Width = 320, Text = prompt, ForeColor = UIStyles.TextPrimary, BackColor = Color.Transparent };
        var combo = new Guna.UI2.WinForms.Guna2ComboBox
        {
            Location = new Point(16, 44), Size = new Size(320, 34),
            DropDownStyle = ComboBoxStyle.DropDownList,
            FillColor = Color.FromArgb(40, 255, 255, 255),
            BorderColor = Color.FromArgb(50, 255, 255, 255),
            BorderRadius = 8,
            ForeColor = UIStyles.TextPrimary
        };
        combo.Items.AddRange(options);
        if (Array.IndexOf(options, defaultOption) >= 0)
            combo.SelectedItem = defaultOption;
        else
            combo.SelectedIndex = 0;
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
        panel.Controls.AddRange(new Control[] { label, combo, okBtn, cancelBtn });
        form.Controls.Add(panel);
        form.AcceptButton = okBtn;
        form.CancelButton = cancelBtn;
        form.Load += (_, _) => { if (Environment.OSVersion.Version.Build >= 22000) UIStyles.EnableAcrylic(form.Handle); };
        return form.ShowDialog() == DialogResult.OK ? combo.SelectedItem?.ToString() : null;
    }
}
