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
                Console.WriteLine("   WIND TURBINE DATA SERVICE - SERVER");
                Console.WriteLine("===========================================");
                Console.WriteLine();

                // Kreiranje ServiceHost-a
                host = new ServiceHost(typeof(WindTurbineService));

                // Otvaranje servisa
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

                // Čekanje na Enter
                Console.ReadLine();

                // Zatvaranje servisa
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
    }
}