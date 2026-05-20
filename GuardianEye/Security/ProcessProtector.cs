using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GuardianEye.Security
{
    public interface IProcessProtector
    {
        Task<bool> KillProcessByNameAsync(string processName);
        Task<bool> IsProcessRunningAsync(string processName);
        Task PreventTaskManagerAsync();
        Task PreventRegistryEditorAsync();
        Task PreventCmdAsync();
    }

    public class ProcessProtector : IProcessProtector
    {
        public async Task<bool> KillProcessByNameAsync(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName);
                foreach (var process in processes)
                {
                    process.Kill();
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsProcessRunningAsync(string processName)
        {
            try
            {
                return Process.GetProcessesByName(processName).Length > 0;
            }
            catch
            {
                return false;
            }
        }

        public async Task PreventTaskManagerAsync()
        {
            await KillProcessByNameAsync("taskmgr");
        }

        public async Task PreventRegistryEditorAsync()
        {
            await KillProcessByNameAsync("regedit");
        }

        public async Task PreventCmdAsync()
        {
            await KillProcessByNameAsync("cmd");
            await KillProcessByNameAsync("powershell");
        }
    }
}