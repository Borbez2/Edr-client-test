using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Edr_client_test
{
    /// <summary>
    /// Retrieves the list of running process names.
    /// </summary>
    public static class ProcessMonitor
    {
        public static List<string> GetRunningProcessNames()
        {
            return Process.GetProcesses()
                .Where(p => !string.IsNullOrEmpty(p.ProcessName))
                .Select(p => p.ProcessName.ToLower())
                .ToList();
        }
    }
}
