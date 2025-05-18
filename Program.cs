using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace EdrClient
{
    internal class Program
    {
        static readonly string[] WatchFolders = {
            @"C:\Payments\Invoices\",
            @"C:\Returns\Claims\"
        };

        static readonly string[] WatchedProcesses = {
            "notepad.exe", "mspaint.exe"
        };

        static async Task Main(string[] args)
        {
            // Watch for processes
            Task.Run(() => WatchProcesses());

            // Watch folders
            foreach (var folder in WatchFolders)
            {
                if (Directory.Exists(folder))
                {
                    var watcher = new FileSystemWatcher(folder)
                    {
                        IncludeSubdirectories = true,
                        EnableRaisingEvents = true
                    };

                    watcher.Created += async (s, e) => await HandleFileEvent("created", e.FullPath);
                    watcher.Changed += async (s, e) => await HandleFileEvent("modified", e.FullPath);
                }
            }

            Console.WriteLine("Monitoring started. Press Enter to stop.");
            Console.ReadLine();
        }

        static async void WatchProcesses()
        {
            while (true)
            {
                var processes = Process.GetProcesses();
                foreach (var proc in processes)
                {
                    try
                    {
                        string name = proc.ProcessName.ToLower();
                        if (Array.Exists(WatchedProcesses, p => p == name))
                        {
                            var eventObj = new
                            {
                                event_type = "process",
                                name = proc.ProcessName,
                                id = proc.Id,
                                start_time = proc.StartTime.ToString("o"),
                                timestamp = DateTime.UtcNow.ToString("o"),
                                host = Environment.MachineName
                            };
                            string json = JsonConvert.SerializeObject(eventObj, Formatting.Indented);
                            await SendToLogstash(json);
                        }
                    }
                    catch { } // Ignore access denied
                }

                await Task.Delay(5000); // poll every 5 seconds
            }
        }

        static async Task HandleFileEvent(string changeType, string filePath)
        {
            var eventObj = new
            {
                event_type = "file",
                change = changeType,
                path = filePath,
                timestamp = DateTime.UtcNow.ToString("o"),
                host = Environment.MachineName
            };

            string json = JsonConvert.SerializeObject(eventObj, Formatting.Indented);
            await SendToLogstash(json);
        }

        static async Task SendToLogstash(string json)
        {
            using var client = new HttpClient();
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var logstashUrl = "http://localhost:5044"; // HTTPS if configured

            try
            {
                var response = await client.PostAsync(logstashUrl, content);
                Console.WriteLine($"[+] Sent to Logstash: {response.StatusCode}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error: {ex.Message}");
            }
        }
    }
}
