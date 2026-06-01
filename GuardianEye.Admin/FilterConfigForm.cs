using GuardianEye.Shared;

namespace GuardianEye.Admin;

public partial class FilterConfigForm : Form
{
    private readonly SessionManager _sessionManager;

    public FilterConfigForm(SessionManager sessionManager)
    {
        InitializeComponent();
        _sessionManager = sessionManager;
        Load += (_, _) =>
        {
            if (Environment.OSVersion.Version.Build >= 22000)
                UIStyles.EnableMica(Handle);
            UIStyles.EnableRoundedCorners(Handle);
        };
    }

    private void buttonApply_Click(object sender, EventArgs e)
    {
        var domains = textBoxDomains.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(d => d.Trim().ToLower())
            .Where(d => !string.IsNullOrEmpty(d) && !d.StartsWith("#"))
            .Distinct()
            .ToList();

        var processes = textBoxProcesses.Text
            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(p => p.Trim())
            .Where(p => !string.IsNullOrEmpty(p) && !p.StartsWith("#"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        var update = new FilterUpdateMessage
        {
            BlockedDomains = domains,
            BlockedProcesses = processes
        };

        _sessionManager.BroadcastMessage(update);

        MessageBox.Show($"Filter sent to {_sessionManager.Clients.Count} connected client(s).\n\n" +
                        $"Domains: {domains.Count}\nProcesses: {processes.Count}",
                        "Filters Applied", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void buttonClose_Click(object sender, EventArgs e)
    {
        Close();
    }
}
