using System;
using System.IO;

namespace Edr_client_test
{
    /// <summary>
    /// Watches a folder for newly created or modified files.
    /// </summary>
    public class FileMonitor
    {
        private readonly FileSystemWatcher _watcher;

        public FileMonitor(string path, Action<string> onChanged)
        {
            _watcher = new FileSystemWatcher(path)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
                IncludeSubdirectories = true,
                EnableRaisingEvents = true
            };

            _watcher.Created += (s, e) => onChanged(e.FullPath);
            _watcher.Changed += (s, e) => onChanged(e.FullPath);
        }
    }
}
