using System;
using System.ServiceModel;
using Server.Services;

namespace Server
{
    class Program
    {
        static void Main(string[] args)
        {
            ServiceHost host = null;

            try
            {
                Console.WriteLine("===========================================");
                Console.WriteLine($"Working Directory: {Environment.CurrentDirectory}");
                Console.WriteLine($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}");
                Console.WriteLine("===========================================");
                Console.WriteLine();

                Console.WriteLine("===========================================");
                Console.WriteLine("   WIND TURBINE DATA SERVICE - SERVER");
                Console.WriteLine("===========================================");
                Console.WriteLine();

                SubscribeToEvents();

                host = new ServiceHost(typeof(WindTurbineService));

                host.Open();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Service started successfully!");
                Console.WriteLine();
                Console.WriteLine("Endpoint:");
                foreach (var endpoint in host.Description.Endpoints)
                {
                    Console.WriteLine($"  - {endpoint.Address}");
                    Console.WriteLine($"    Binding: {endpoint.Binding.Name}");
                    Console.WriteLine($"    Contract: {endpoint.Contract.Name}");
                    Console.WriteLine();
                }

                Console.WriteLine("===========================================");
                Console.WriteLine("Server is running...");
                Console.WriteLine("Press ENTER to stop the server.");
                Console.WriteLine("===========================================");
                Console.WriteLine();

                Console.ReadLine();

                Console.WriteLine();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Shutting down server...");
                host.Close();
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Server stopped.");
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine();
                Console.WriteLine($"[ERROR] Communication error: {ex.Message}");
                if (host != null)
                {
                    host.Abort();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine($"[ERROR] Unexpected error: {ex.Message}");
                Console.WriteLine($"Details: {ex.StackTrace}");
                if (host != null)
                {
                    host.Abort();
                }
            }
            finally
            {
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }

        private static void SubscribeToEvents()
        {
            var eventPublisher = Events.EventPublisher.Instance;

            eventPublisher.OnTransferStarted += (sender, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine($"║  TRANSFER STARTED                      ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine($"║  Turbine: {e.TurbineId,-28} ║");
                Console.WriteLine($"║  File:    {e.FileName,-28} ║");
                Console.WriteLine($"║  Time:    {e.StartTime:yyyy-MM-dd HH:mm:ss}        ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine();
            };

            eventPublisher.OnSampleReceived += (sender, e) =>
            {
                Console.WriteLine($"[EVENT] Sample milestone: {e.SampleCount} samples received from {e.TurbineId}");
            };

            eventPublisher.OnTransferCompleted += (sender, e) =>
            {
                Console.WriteLine();
                Console.WriteLine("╔════════════════════════════════════════╗");
                Console.WriteLine($"║  TRANSFER COMPLETED                    ║");
                Console.WriteLine("╠════════════════════════════════════════╣");
                Console.WriteLine($"║  Turbine:  {e.TurbineId,-27} ║");
                Console.WriteLine($"║  Received: {e.TotalSamplesReceived,-27} ║");
                Console.WriteLine($"║  Rejected: {e.TotalSamplesRejected,-27} ║");
                Console.WriteLine($"║  Time:     {e.EndTime:yyyy-MM-dd HH:mm:ss}        ║");
                Console.WriteLine("╚════════════════════════════════════════╝");
                Console.WriteLine();
            };

            eventPublisher.OnWarningRaised += (sender, e) =>
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"[WARNING] [{e.WarningType}] {e.TurbineId} @ {e.Timestamp:HH:mm:ss}: {e.Message}");
                Console.ResetColor();
            };

            Console.WriteLine("[INFO] Event subscriptions initialized.");
        }
    }
}