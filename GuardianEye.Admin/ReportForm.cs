using System.Text;

namespace GuardianEye.Admin;

public partial class ReportForm : Form
{
    private readonly DatabaseService _db;

    public ReportForm(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
            UIStyles.EnableRoundedCorners(Handle);
        };
        LoadSessions();
        LoadBlockedAttempts();
        LoadSummary();
    }

    private void LoadSessions()
    {
        dataGridViewSessions.Rows.Clear();
        foreach (var s in _db.GetAllSessions())
            dataGridViewSessions.Rows.Add(s.DisplayName, s.Username, s.StartTime.ToString("yyyy-MM-dd HH:mm"), s.DurationSeconds / 60, s.Status);
    }

    private void LoadBlockedAttempts()
    {
        dataGridViewAttempts.Rows.Clear();
        foreach (var a in _db.GetBlockedAttempts())
            dataGridViewAttempts.Rows.Add(a.DisplayName, a.Username, a.AttemptType, a.TargetName, a.Timestamp.ToString("yyyy-MM-dd HH:mm"));
    }

    private void LoadSummary()
    {
        var today = _db.GetTodaySummary();
        labelSummary.Text = $"Today: {today.TotalStudents} students, {today.TotalMinutes} minutes, {today.ActiveSessions} active";

        dataGridViewActive.Rows.Clear();
        foreach (var a in _db.GetMostActiveStudents(10))
            dataGridViewActive.Rows.Add(a.DisplayName, a.Username, a.TotalMinutes, a.SessionCount);
    }

    private void buttonExportSessions_Click(object sender, EventArgs e)
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"GuardianEye_Sessions_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (sfd.ShowDialog() == DialogResult.OK)
        {
            _db.ExportSessionsToCsv(sfd.FileName);
            MessageBox.Show($"Exported to {sfd.FileName}", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

    private void tabControlReports_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabControlReports.SelectedIndex == 0)
            LoadSessions();
        else if (tabControlReports.SelectedIndex == 1)
            LoadBlockedAttempts();
        else if (tabControlReports.SelectedIndex == 2)
            LoadSummary();
    }

    private void buttonRefresh_Click(object sender, EventArgs e)
    {
        LoadSessions();
        LoadBlockedAttempts();
        LoadSummary();
    }
}
