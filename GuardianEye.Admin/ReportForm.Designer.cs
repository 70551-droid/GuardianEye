namespace GuardianEye.Admin;

partial class ReportForm
{
    private System.ComponentModel.IContainer components = null;
    private TabControl tabControlReports;
    private TabPage tabPageSessions;
    private TabPage tabPageAttempts;
    private TabPage tabPageSummary;
    private Guna.UI2.WinForms.Guna2DataGridView dataGridViewSessions;
    private Guna.UI2.WinForms.Guna2DataGridView dataGridViewAttempts;
    private Guna.UI2.WinForms.Guna2DataGridView dataGridViewActive;
    private Guna.UI2.WinForms.Guna2Button buttonExportSessions;
    private Guna.UI2.WinForms.Guna2Button buttonRefresh;
    private Label labelSummary;

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
        grid.ReadOnly = true;
    }

    private void InitializeComponent()
    {
        tabControlReports = new TabControl();
        tabPageSessions = new TabPage();
        tabPageAttempts = new TabPage();
        tabPageSummary = new TabPage();
        dataGridViewSessions = new Guna.UI2.WinForms.Guna2DataGridView();
        dataGridViewAttempts = new Guna.UI2.WinForms.Guna2DataGridView();
        dataGridViewActive = new Guna.UI2.WinForms.Guna2DataGridView();
        buttonExportSessions = new Guna.UI2.WinForms.Guna2Button();
        buttonRefresh = new Guna.UI2.WinForms.Guna2Button();
        labelSummary = new Label();

        tabControlReports.SuspendLayout();
        tabPageSessions.SuspendLayout();
        tabPageAttempts.SuspendLayout();
        tabPageSummary.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dataGridViewSessions).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewAttempts).BeginInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewActive).BeginInit();
        SuspendLayout();

        // TabControl
        tabControlReports.Controls.Add(tabPageSessions);
        tabControlReports.Controls.Add(tabPageAttempts);
        tabControlReports.Controls.Add(tabPageSummary);
        tabControlReports.Location = new Point(12, 12);
        tabControlReports.Size = new Size(680, 380);
        tabControlReports.SelectedIndexChanged += tabControlReports_SelectedIndexChanged;
        tabControlReports.BackColor = UIStyles.MidBg;
        tabControlReports.ForeColor = UIStyles.TextPrimary;

        // Tab Pages
        tabPageSessions.Text = "  Sessions  ";
        tabPageSessions.BackColor = UIStyles.GlassCard;
        tabPageSessions.UseVisualStyleBackColor = false;

        tabPageAttempts.Text = "  Blocked  ";
        tabPageAttempts.BackColor = UIStyles.GlassCard;
        tabPageAttempts.UseVisualStyleBackColor = false;

        tabPageSummary.Text = "  Summary  ";
        tabPageSummary.BackColor = UIStyles.GlassCard;
        tabPageSummary.UseVisualStyleBackColor = false;

        // Sessions grid
        StyleGrid(dataGridViewSessions);
        dataGridViewSessions.Columns.Add("Name", "Student");
        dataGridViewSessions.Columns.Add("Username", "Username");
        dataGridViewSessions.Columns.Add("Start", "Start Time");
        dataGridViewSessions.Columns.Add("Duration", "Duration (min)");
        dataGridViewSessions.Columns.Add("Status", "Status");
        dataGridViewSessions.Dock = DockStyle.Fill;
        dataGridViewSessions.Location = new Point(3, 3);
        dataGridViewSessions.Size = new Size(528, 346);
        tabPageSessions.Controls.Add(dataGridViewSessions);

        // Blocked attempts grid
        StyleGrid(dataGridViewAttempts);
        dataGridViewAttempts.Columns.Add("Name", "Student");
        dataGridViewAttempts.Columns.Add("Username", "Username");
        dataGridViewAttempts.Columns.Add("Type", "Type");
        dataGridViewAttempts.Columns.Add("Target", "Target");
        dataGridViewAttempts.Columns.Add("Time", "Time");
        dataGridViewAttempts.Dock = DockStyle.Fill;
        dataGridViewAttempts.Location = new Point(3, 3);
        dataGridViewAttempts.Size = new Size(528, 346);
        tabPageAttempts.Controls.Add(dataGridViewAttempts);

        // Summary tab
        labelSummary.AutoSize = true;
        labelSummary.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        labelSummary.ForeColor = UIStyles.TextPrimary;
        labelSummary.BackColor = Color.Transparent;
        labelSummary.Location = new Point(10, 10);
        labelSummary.Size = new Size(300, 20);
        labelSummary.Text = "Today: 0 students, 0 minutes, 0 active";

        StyleGrid(dataGridViewActive);
        dataGridViewActive.Columns.Add("Name", "Student");
        dataGridViewActive.Columns.Add("Username", "Username");
        dataGridViewActive.Columns.Add("TotalMinutes", "Total Minutes");
        dataGridViewActive.Columns.Add("Sessions", "Sessions");
        dataGridViewActive.Location = new Point(10, 40);
        dataGridViewActive.Size = new Size(500, 290);

        tabPageSummary.Controls.Add(labelSummary);
        tabPageSummary.Controls.Add(dataGridViewActive);

        // Export button
        buttonExportSessions.Size = new Size(120, 36);
        buttonExportSessions.Location = new Point(12, 400);
        buttonExportSessions.Text = "Export CSV";
        UIStyles.StyleGunaButton(buttonExportSessions, UIStyles.AccentBlue);
        buttonExportSessions.Click += buttonExportSessions_Click;

        // Refresh button
        buttonRefresh.Size = new Size(90, 36);
        buttonRefresh.Location = new Point(140, 400);
        buttonRefresh.Text = "Refresh";
        UIStyles.StyleGunaButton(buttonRefresh, Color.FromArgb(80, 80, 100));
        buttonRefresh.Click += buttonRefresh_Click;

        // Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = UIStyles.DeepBg;
        ClientSize = new Size(704, 451);
        Controls.AddRange(new Control[] { tabControlReports, buttonExportSessions, buttonRefresh });
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ReportForm";
        StartPosition = FormStartPosition.CenterParent;
        Text = "GuardianEye - Reports & Analytics";
        tabControlReports.ResumeLayout(false);
        tabPageSessions.ResumeLayout(false);
        tabPageAttempts.ResumeLayout(false);
        tabPageSummary.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dataGridViewSessions).EndInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewAttempts).EndInit();
        ((System.ComponentModel.ISupportInitialize)dataGridViewActive).EndInit();
        ResumeLayout(false);
    }
}
