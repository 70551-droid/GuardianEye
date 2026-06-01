namespace GuardianEye.Admin;

partial class GroupManagementForm
{
    private System.ComponentModel.IContainer components = null;
    private Guna.UI2.WinForms.Guna2DataGridView dataGridViewGroups;
    private ListBox listBoxStudents;
    private Guna.UI2.WinForms.Guna2Button buttonAddGroup;
    private Guna.UI2.WinForms.Guna2Button buttonEditGroup;
    private Guna.UI2.WinForms.Guna2Button buttonDeleteGroup;
    private Guna.UI2.WinForms.Guna2Button buttonEditStudent;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    void StyleGrid(Guna.UI2.WinForms.Guna2DataGridView grid)
    {
        grid.AllowUserToAddRows = false;
        grid.AllowUserToDeleteRows = false;
        grid.BackgroundColor = UIStyles.GlassCard;
        grid.ColumnHeadersHeight = 32;
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.BorderStyle = BorderStyle.None;
        grid.RowHeadersVisible = false;
        grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        grid.GridColor = Color.FromArgb(50, 255, 255, 255);
        grid.ForeColor = UIStyles.TextPrimary;
        grid.RowTemplate.Height = 28;
        grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(40, 40, 60);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = UIStyles.TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
    }

    private void InitializeComponent()
    {
        dataGridViewGroups = new Guna.UI2.WinForms.Guna2DataGridView();
        listBoxStudents = new ListBox();
        buttonAddGroup = new Guna.UI2.WinForms.Guna2Button();
        buttonEditGroup = new Guna.UI2.WinForms.Guna2Button();
        buttonDeleteGroup = new Guna.UI2.WinForms.Guna2Button();
        buttonEditStudent = new Guna.UI2.WinForms.Guna2Button();

        ((System.ComponentModel.ISupportInitialize)dataGridViewGroups).BeginInit();
        SuspendLayout();

        var labelGroups = new Label();
        labelGroups.AutoSize = true;
        labelGroups.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelGroups.ForeColor = UIStyles.TextPrimary;
        labelGroups.BackColor = Color.Transparent;
        labelGroups.Location = new Point(12, 12);
        labelGroups.Size = new Size(140, 21);
        labelGroups.Text = "Groups";

        StyleGrid(dataGridViewGroups);
        dataGridViewGroups.Columns.Add("Id", "ID");
        dataGridViewGroups.Columns.Add("Name", "Name");
        dataGridViewGroups.Columns.Add("Limit", "Daily Limit (h)");
        dataGridViewGroups.Columns.Add("Students", "Students");
        dataGridViewGroups.Location = new Point(12, 40);
        dataGridViewGroups.Size = new Size(460, 140);
        dataGridViewGroups.Columns[0].Visible = false;

        buttonAddGroup.Size = new Size(100, 32);
        buttonAddGroup.Location = new Point(12, 190);
        buttonAddGroup.Text = "Add Group";
        UIStyles.StyleGunaButton(buttonAddGroup, UIStyles.AccentGreen);
        buttonAddGroup.Click += buttonAddGroup_Click;

        buttonEditGroup.Size = new Size(100, 32);
        buttonEditGroup.Location = new Point(120, 190);
        buttonEditGroup.Text = "Edit Group";
        UIStyles.StyleGunaButton(buttonEditGroup, UIStyles.AccentBlue);
        buttonEditGroup.Click += buttonEditGroup_Click;

        buttonDeleteGroup.Size = new Size(100, 32);
        buttonDeleteGroup.Location = new Point(228, 190);
        buttonDeleteGroup.Text = "Delete Group";
        UIStyles.StyleGunaButton(buttonDeleteGroup, UIStyles.AccentRed);
        buttonDeleteGroup.Click += buttonDeleteGroup_Click;

        var labelStudents = new Label();
        labelStudents.AutoSize = true;
        labelStudents.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        labelStudents.ForeColor = UIStyles.TextPrimary;
        labelStudents.BackColor = Color.Transparent;
        labelStudents.Location = new Point(12, 235);
        labelStudents.Size = new Size(160, 21);
        labelStudents.Text = "Students";

        listBoxStudents.BackColor = Color.FromArgb(25, 25, 45);
        listBoxStudents.ForeColor = UIStyles.TextPrimary;
        listBoxStudents.Font = new Font("Consolas", 9F);
        listBoxStudents.FormattingEnabled = true;
        listBoxStudents.ItemHeight = 14;
        listBoxStudents.Location = new Point(12, 265);
        listBoxStudents.Size = new Size(460, 130);
        listBoxStudents.BorderStyle = BorderStyle.None;

        buttonEditStudent.Size = new Size(110, 32);
        buttonEditStudent.Location = new Point(12, 405);
        buttonEditStudent.Text = "Edit Student";
        UIStyles.StyleGunaButton(buttonEditStudent, UIStyles.AccentBlue);
        buttonEditStudent.Click += buttonEditStudent_Click;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(484, 450);
        Controls.AddRange(new Control[] {
            labelGroups, dataGridViewGroups,
            buttonAddGroup, buttonEditGroup, buttonDeleteGroup,
            labelStudents, listBoxStudents, buttonEditStudent
        });
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "GroupManagementForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "GuardianEye - Groups & Students";
        ((System.ComponentModel.ISupportInitialize)dataGridViewGroups).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }
}
