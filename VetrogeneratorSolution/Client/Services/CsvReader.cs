using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Common.DataModels;

namespace Client.Services
{
    public class CsvReader
    {
        private const int HEADER_ROW = 10; // Red 10 (index 9) je header
        private const int DATA_START_ROW = 11; // Podaci počinju od reda 11 (index 10)

        // Indeksi kolona koje nas zanimaju (0-based)
        private const int COL_TIMESTAMP = 0;
        private const int COL_WIND_SPEED = 1;
        private const int COL_WIND_DIRECTION = 14;
        private const int COL_NACELLE_POSITION = 15;
        private const int COL_POWER_KW = 61;
        private const int COL_POTENTIAL_POWER_KW = 62;
        private const int COL_POWER_FACTOR = 81;
        private const int COL_REACTIVE_POWER_KVAR = 85;
        private const int COL_GRID_FREQUENCY_HZ = 268;
        private const int COL_GENERATOR_RPM = 224;

        public List<WindTurbineSample> ReadCsvFile(string filePath, string turbineId, out List<string> errors)
        {
            var samples = new List<WindTurbineSample>();
            errors = new List<string>();

            if (!File.Exists(filePath))
            {
                errors.Add($"File not found: {filePath}");
                return samples;
            }

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string line;
                    int lineNumber = 0;
                    string[] headers = null;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;

                        // Skip prvih 9 redova (komentari/metadata)
                        if (lineNumber < HEADER_ROW)
                            continue;

                        // Red 10 je header
                        if (lineNumber == HEADER_ROW)
                        {
                            headers = line.Split(',');
                            Console.WriteLine($"[INFO] Header found: {headers.Length} columns");
                            continue;
                        }

                        // Od reda 11 - parsiranje podataka
                        try
                        {
                            var sample = ParseLine(line, lineNumber, turbineId);
                            if (sample != null)
                            {
                                samples.Add(sample);
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Row {lineNumber}: {ex.Message}";
                            errors.Add(errorMsg);
                            // Nastavi sa sledećim redom
                        }
                    }
                }

                Console.WriteLine($"[INFO] Successfully parsed {samples.Count} samples from {Path.GetFileName(filePath)}");
                if (errors.Count > 0)
                {
                    Console.WriteLine($"[WARNING] {errors.Count} rows had parsing errors");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"File reading error: {ex.Message}");
            }

            return samples;
        }

        private WindTurbineSample ParseLine(string line, int lineNumber, string turbineId)
        {
            string[] fields = line.Split(',');

            // Provera broja kolona
            if (fields.Length < 270) // Minimalno kolona koje nam trebaju
            {
                throw new FormatException($"Insufficient columns: {fields.Length}");
            }

            var sample = new WindTurbineSample
            {
                RowIndex = lineNumber,
                TurbineId = turbineId
            };

            // Parsiranje sa InvariantCulture (decimalna tačka)
            try
            {
                // Timestamp
                sample.Timestamp = DateTime.Parse(fields[COL_TIMESTAMP], CultureInfo.InvariantCulture);

                // Numeričke vrednosti - provera za NaN
                sample.WindSpeed = ParseDoubleOrThrow(fields[COL_WIND_SPEED], "WindSpeed");
                sample.WindDirection = ParseDoubleOrThrow(fields[COL_WIND_DIRECTION], "WindDirection");
                sample.NacellePosition = ParseDoubleOrThrow(fields[COL_NACELLE_POSITION], "NacellePosition");
                sample.PowerKW = ParseDoubleOrThrow(fields[COL_POWER_KW], "PowerKW");
                sample.PotentialPowerDefaultKW = ParseDoubleOrThrow(fields[COL_POTENTIAL_POWER_KW], "PotentialPowerDefaultKW");
                sample.PowerFactor = ParseDoubleOrThrow(fields[COL_POWER_FACTOR], "PowerFactor");
                sample.ReactivePowerKvar = ParseDoubleOrThrow(fields[COL_REACTIVE_POWER_KVAR], "ReactivePowerKvar");
                sample.GridFrequencyHz = ParseDoubleOrThrow(fields[COL_GRID_FREQUENCY_HZ], "GridFrequencyHz");
                sample.GeneratorRpm = ParseDoubleOrThrow(fields[COL_GENERATOR_RPM], "GeneratorRpm");
            }
            catch (Exception ex)
            {
                throw new FormatException($"Parsing error: {ex.Message}");
            }

            return sample;
        }

        private double ParseDoubleOrThrow(string value, string fieldName)
        {
            // Provera za NaN ili prazno
            if (string.IsNullOrWhiteSpace(value) || value.Trim().Equals("NaN", StringComparison.OrdinalIgnoreCase))
            {
                throw new FormatException($"{fieldName} is NaN or empty");
            }

            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result))
            {
                throw new FormatException($"{fieldName} invalid format: {value}");
            }

            return result;
        }

        public static List<string> GetAvailableCsvFiles(string dataPath)
        {
            var files = new List<string>();

            if (!Directory.Exists(dataPath))
            {
                Console.WriteLine($"[WARNING] Data directory not found: {dataPath}");
                return files;
            }

            // Pronađi sve Kelmarsh_X.csv fajlove
            for (int i = 1; i <= 6; i++)
            {
                string fileName = $"Kelmarsh_{i}.csv";
                string fullPath = Path.Combine(dataPath, fileName);

                if (File.Exists(fullPath))
                {
                    files.Add(fullPath);
                }
            }

            return files;
        }
    }
}
