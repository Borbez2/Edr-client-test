using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace Edr_client_test
{
    class Program
    {
        // Keywords for process names
        static readonly string[] processKeywords = { "notepad", "zoom", "excel", "mspaint", "outlook" };
        // Keywords for filenames
        static readonly string[] fileKeywords = { "invoice", "claim", "resume", "budget" };
        // Folders to watch
        static readonly string[] foldersToWatch = {
            @"C:\Payments\Invoices",
            @"C:\Returns\Claims",
            @"C:\HR\Resumes",
            @"C:\Finance\Reports"
        };

        static async Task Main()
        {
            var sender = new JsonSender("http://localhost:5044");

            // Set up file watchers
            foreach (var folder in foldersToWatch)
            {
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                new FileMonitor(folder, async filePath =>
                {
                    string filename = Path.GetFileName(filePath).ToLower();
                    if (fileKeywords.Any(k => filename.Contains(k)))
                    {
                        var log = new
                        {
                            event_type = "file",
                            filename,
                            path = filePath,
                            timestamp = DateTime.UtcNow,
                            host = Environment.MachineName
                        };
                        string json = JsonSerializer.Serialize(log);
                        await sender.SendAsync(json);
                    }
                });
            }

            // Start process monitoring loop
            _ = Task.Run(async () =>
            {
                while (true)
                {
                    var processes = ProcessMonitor.GetRunningProcessNames();
                    foreach (var proc in processes)
                    {
                        if (processKeywords.Any(k => proc.Contains(k)))
                        {
                            var log = new
                            {
                                event_type = "process",
                                name = proc,
                                timestamp = DateTime.UtcNow,
                                host = Environment.MachineName
                            };
                            string json = JsonSerializer.Serialize(log);
                            await sender.SendAsync(json);
                        }
                    }
                    await Task.Delay(3000);
                }
            });

            Console.WriteLine("[*] Monitoring started. Press ENTER to exit.");
            Console.ReadLine();
        }
    }
}
