using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using Client.Proxy;
using Client.Services;
using Common.DataModels;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("===========================================");
            Console.WriteLine("  WIND TURBINE DATA CLIENT");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            string dataPath = ConfigurationManager.AppSettings["DataPath"];

            if (!Path.IsPathRooted(dataPath))
            {
                dataPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataPath));
            }

            Console.WriteLine($"Data folder: {dataPath}");
            Console.WriteLine();

            var availableFiles = CsvReader.GetAvailableCsvFiles(dataPath);

            if (availableFiles.Count == 0)
            {
                Console.WriteLine("[ERROR] No CSV files found in Data folder!");
                Console.WriteLine($"Expected files: Kelmarsh_1.csv to Kelmarsh_6.csv");
                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Found {availableFiles.Count} CSV file(s):");
            for (int i = 0; i < availableFiles.Count; i++)
            {
                Console.WriteLine($"  [{i + 1}] {Path.GetFileName(availableFiles[i])}");
            }
            Console.WriteLine();

            int selectedIndex = -1;
            while (selectedIndex < 0 || selectedIndex >= availableFiles.Count)
            {
                Console.Write($"Select file (1-{availableFiles.Count}): ");
                string input = Console.ReadLine();

                if (int.TryParse(input, out int choice) && choice >= 1 && choice <= availableFiles.Count)
                {
                    selectedIndex = choice - 1;
                }
                else
                {
                    Console.WriteLine("[ERROR] Invalid choice. Try again.");
                }
            }

            string selectedFile = availableFiles[selectedIndex];
            string fileName = Path.GetFileName(selectedFile);
            string turbineId = fileName.Replace("Kelmarsh_", "").Replace(".csv", ""); // Npr. "1", "2", ...

            Console.WriteLine();
            Console.WriteLine($"Selected: {fileName} (Turbine ID: {turbineId})");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            Console.WriteLine("Reading CSV file...");
            var csvReader = new CsvReader();
            List<string> parsingErrors;
            var samples = csvReader.ReadCsvFile(selectedFile, turbineId, out parsingErrors);

            if (samples.Count == 0)
            {
                Console.WriteLine("[ERROR] No valid samples found in file!");

                if (parsingErrors.Count > 0)
                {
                    Console.WriteLine();
                    Console.WriteLine("Parsing errors:");
                    foreach (var error in parsingErrors.Take(10)) // Prikaži prvih 10
                    {
                        Console.WriteLine($"  - {error}");
                    }
                    if (parsingErrors.Count > 10)
                    {
                        Console.WriteLine($"  ... and {parsingErrors.Count - 10} more errors");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine($"Loaded {samples.Count} valid samples.");
            Console.WriteLine();

            Console.Write("Send data to server? (y/n): ");
            string confirm = Console.ReadLine();

            if (confirm?.ToLower() != "y")
            {
                Console.WriteLine("Transfer cancelled.");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
                return;
            }

            Console.WriteLine();
            Console.WriteLine("===========================================");
            Console.WriteLine("Starting data transfer...");
            Console.WriteLine("===========================================");
            Console.WriteLine();

            try
            {
                using (var proxy = new ServiceProxy())
                {
                    var metadata = new SessionMetadata(
                        turbineId: $"Kelmarsh_{turbineId}",
                        startTime: samples.First().Timestamp,
                        fileName: fileName
                    );

                    proxy.StartSession(metadata);
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session started.");
                    Console.WriteLine();

                    int successCount = 0;
                    int errorCount = 0;

                    for (int i = 0; i < samples.Count; i++)
                    {
                        try
                        {
                            proxy.PushSample(samples[i]);
                            successCount++;

                            if ((i + 1) % 500 == 0)
                            {
                                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Progress: {i + 1}/{samples.Count} samples sent...");
                            }
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                        }
                    }

                    Console.WriteLine();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Transfer completed!");
                    Console.WriteLine($"  - Sent: {successCount} samples");
                    Console.WriteLine($"  - Errors: {errorCount} samples");
                    Console.WriteLine();

                    proxy.EndSession();
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session ended.");
                }

                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine("SUCCESS: Data transfer completed!");
                Console.WriteLine("===========================================");
            }
            catch (Exception ex)
            {
                Console.WriteLine();
                Console.WriteLine("===========================================");
                Console.WriteLine("[ERROR] Transfer failed!");
                Console.WriteLine($"Reason: {ex.Message}");
                Console.WriteLine("===========================================");
            }

            Console.WriteLine();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
