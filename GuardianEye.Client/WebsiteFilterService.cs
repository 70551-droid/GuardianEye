using System.Text;

namespace GuardianEye.Client;

public class WebsiteFilterService : IDisposable
{
    private static readonly string HostsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.System), "drivers", "etc", "hosts");

    private static readonly string MarkerStart = "# === GuardianEye Blocked Sites START ===";
    private static readonly string MarkerEnd = "# === GuardianEye Blocked Sites END ===";

    private List<string> _blockedDomains = new();
    private bool _isDisposed;

    public void UpdateBlockedDomains(List<string> domains)
    {
        _blockedDomains = domains ?? new List<string>();
        ApplyHostsFile();
    }

    private void ApplyHostsFile()
    {
        try
        {
            string content;
            if (File.Exists(HostsFilePath))
                content = File.ReadAllText(HostsFilePath);
            else
                content = "";

            // Remove existing GuardianEye block section
            int startIdx = content.IndexOf(MarkerStart);
            int endIdx = content.IndexOf(MarkerEnd);
            if (startIdx >= 0 && endIdx >= 0)
            {
                content = content.Remove(startIdx, endIdx + MarkerEnd.Length - startIdx).TrimEnd();
                content += Environment.NewLine;
            }

            // Add new block section if there are domains to block
            if (_blockedDomains.Count > 0)
            {
                var sb = new StringBuilder();
                sb.AppendLine(MarkerStart);
                foreach (string domain in _blockedDomains)
                {
                    string trimmed = domain.Trim().ToLower();
                    if (string.IsNullOrEmpty(trimmed)) continue;
                    sb.AppendLine($"127.0.0.1 {trimmed}");
                    sb.AppendLine($"127.0.0.1 www.{trimmed}");
                }
                sb.AppendLine(MarkerEnd);
                content += sb.ToString();
            }

            File.WriteAllText(HostsFilePath, content);
        }
        catch (UnauthorizedAccessException)
        {
            // App must run as admin to modify hosts file
        }
        catch { }
    }

    public List<string> GetCurrentBlockedDomains() => new List<string>(_blockedDomains);

    public void Clear()
    {
        _blockedDomains.Clear();
        ApplyHostsFile();
    }

    public void Dispose()
    {
        if (_isDisposed) return;
        _isDisposed = true;
        Clear();
    }
}
