using Edr_client_test;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FileActivityMonitor
{
    class Program
    {
        private static FileMonitor fileMonitor;
        private static ProcessMonitor processMonitor;
        private static CancellationTokenSource cancellationTokenSource;

        static async Task Main(string[] args)
        {
            Console.WriteLine("File & Program Activity Monitoring Agent Starting...");

            try
            {
                // Initialize configuration
                var configManager = new ConfigManager();
                var config = configManager.LoadConfig();

                // Initialize components
                var logger = new Logger();
                var offlineQueue = new OfflineQueue();
                var jsonSender = new JsonSender(config.ServerAddress, offlineQueue, logger);
                var ruleEngine = new RuleEngine(config, logger);

                // Initialize monitors
                fileMonitor = new FileMonitor(config, ruleEngine, jsonSender, logger);
                processMonitor = new ProcessMonitor(config, ruleEngine, jsonSender, logger);

                // Set up cancellation token for graceful shutdown
                cancellationTokenSource = new CancellationTokenSource();
                Console.CancelKeyPress += (sender, e) => {
                    e.Cancel = true;
                    cancellationTokenSource.Cancel();
                    Console.WriteLine("\nShutdown requested. Stopping monitors...");
                };

                // Start monitoring
                var fileMonitorTask = fileMonitor.StartMonitoring(cancellationTokenSource.Token);
                var processMonitorTask = processMonitor.StartMonitoring(cancellationTokenSource.Token);
                var queueProcessorTask = offlineQueue.StartProcessing(jsonSender, cancellationTokenSource.Token);

                Console.WriteLine("Monitoring started. Press Ctrl+C to stop.");

                // Wait for cancellation
                await Task.WhenAny(
                    Task.Delay(-1, cancellationTokenSource.Token),
                    fileMonitorTask,
                    processMonitorTask,
                    queueProcessorTask
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Fatal error: {ex.Message}");
            }
            finally
            {
                // Cleanup
                fileMonitor?.Dispose();
                processMonitor?.Dispose();
                cancellationTokenSource?.Dispose();
                Console.WriteLine("Monitoring agent stopped.");
            }
        }
    }
}