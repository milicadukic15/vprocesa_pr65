using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Common.DataModels;
using Microsoft.VisualBasic.FileIO;

namespace Client.Services
{
    public class CsvReader
    {
        private const int HEADER_ROW = 10; // Red 10 je header
        private const int DATA_START_ROW = 11; // Podaci počinju od reda 11

        // Nazivi kolona koje tražimo
        private const string COL_NAME_TIMESTAMP = "# Date and time";
        private const string COL_NAME_WIND_SPEED = "Wind speed (m/s)";
        private const string COL_NAME_WIND_DIRECTION = "Wind direction (°)";
        private const string COL_NAME_NACELLE_POSITION = "Nacelle position (°)";
        private const string COL_NAME_POWER_KW = "Power (kW)";
        private const string COL_NAME_POTENTIAL_POWER_KW = "Potential power default PC (kW)";
        private const string COL_NAME_POWER_FACTOR = "Power factor (cosphi)";
        private const string COL_NAME_REACTIVE_POWER_KVAR = "Reactive power (kvar)";
        private const string COL_NAME_GRID_FREQUENCY_HZ = "Grid frequency (Hz)";
        private const string COL_NAME_GENERATOR_RPM = "Generator RPM (RPM)";

        private Dictionary<string, int> columnIndexMap;

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

                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;

                        // Skip prvih 9 redova
                        if (lineNumber < HEADER_ROW)
                            continue;

                        // Red 10 je header - kreiraj mapu kolona
                        if (lineNumber == HEADER_ROW)
                        {
                            string[] headers = ParseCsvLine(line);
                            Console.WriteLine($"[INFO] Header found: {headers.Length} columns");

                            columnIndexMap = BuildColumnIndexMap(headers);

                            // Provera da li smo našli sve potrebne kolone
                            if (columnIndexMap.Count < 10)
                            {
                                errors.Add("Missing required columns in CSV header");
                                Console.WriteLine("[ERROR] Could not find all required columns!");
                                return samples;
                            }

                            Console.WriteLine("[INFO] Successfully mapped all required columns.");
                            continue;
                        }

                        // Od reda 11 - parsiranje podataka
                        try
                        {
                            string[] fields = ParseCsvLine(line); // ← Parsiraj prvo
                            var sample = ParseLine(fields, lineNumber, turbineId);
                            if (sample != null)
                            {
                                samples.Add(sample);
                            }

                            if (lineNumber % 1000 == 0)
                            {
                                Console.WriteLine($"[PROGRESS] Processing row {lineNumber}... ({samples.Count} valid samples so far)");
                            }
                        }
                        catch (Exception ex)
                        {
                            string errorMsg = $"Row {lineNumber}: {ex.Message}";
                            errors.Add(errorMsg);
                            // Nastavi sa sledećim redom (ne prekidaj)
                        }
                    }
                }

                Console.WriteLine($"[INFO] Successfully parsed {samples.Count} samples from {Path.GetFileName(filePath)}");
                if (errors.Count > 0)
                {
                    Console.WriteLine($"[WARNING] {errors.Count} rows had parsing errors (NaN values or invalid data)");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"File reading error: {ex.Message}");
            }

            return samples;
        }

        private Dictionary<string, int> BuildColumnIndexMap(string[] headers)
        {
            var map = new Dictionary<string, int>();

            // Pronađi indekse potrebnih kolona
            for (int i = 0; i < headers.Length; i++)
            {
                string header = headers[i].Trim();

                if (header == COL_NAME_TIMESTAMP) map[COL_NAME_TIMESTAMP] = i;
                else if (header == COL_NAME_WIND_SPEED) map[COL_NAME_WIND_SPEED] = i;
                else if (header == COL_NAME_WIND_DIRECTION) map[COL_NAME_WIND_DIRECTION] = i;
                else if (header == COL_NAME_NACELLE_POSITION) map[COL_NAME_NACELLE_POSITION] = i;
                else if (header == COL_NAME_POWER_KW) map[COL_NAME_POWER_KW] = i;
                else if (header == COL_NAME_POTENTIAL_POWER_KW) map[COL_NAME_POTENTIAL_POWER_KW] = i;
                else if (header == COL_NAME_POWER_FACTOR) map[COL_NAME_POWER_FACTOR] = i;
                else if (header == COL_NAME_REACTIVE_POWER_KVAR) map[COL_NAME_REACTIVE_POWER_KVAR] = i;
                else if (header == COL_NAME_GRID_FREQUENCY_HZ) map[COL_NAME_GRID_FREQUENCY_HZ] = i;
                else if (header == COL_NAME_GENERATOR_RPM) map[COL_NAME_GENERATOR_RPM] = i;
            }

            return map;
        }

        private WindTurbineSample ParseLine(string[] fields, int lineNumber, string turbineId)
        {

            var sample = new WindTurbineSample
            {
                RowIndex = lineNumber,
                TurbineId = turbineId
            };

            try
            {
                // Parsiranje koristeći mapu indeksa
                sample.Timestamp = DateTime.Parse(
                    fields[columnIndexMap[COL_NAME_TIMESTAMP]],
                    CultureInfo.InvariantCulture
                );

                sample.WindSpeed = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_WIND_SPEED]], "WindSpeed");
                sample.WindDirection = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_WIND_DIRECTION]], "WindDirection");
                sample.NacellePosition = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_NACELLE_POSITION]], "NacellePosition");
                sample.PowerKW = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_POWER_KW]], "PowerKW");
                sample.PotentialPowerDefaultKW = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_POTENTIAL_POWER_KW]], "PotentialPowerDefaultKW");
                sample.PowerFactor = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_POWER_FACTOR]], "PowerFactor");
                sample.ReactivePowerKvar = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_REACTIVE_POWER_KVAR]], "ReactivePowerKvar");
                sample.GridFrequencyHz = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_GRID_FREQUENCY_HZ]], "GridFrequencyHz");
                sample.GeneratorRpm = ParseDoubleOrThrow(fields[columnIndexMap[COL_NAME_GENERATOR_RPM]], "GeneratorRpm");
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

            // Učitaj sve .csv fajlove
            var allCsvFiles = Directory.GetFiles(dataPath, "*.csv");

            foreach (var file in allCsvFiles)
            {
                files.Add(file);
            }

            return files;
        }

        private string[] ParseCsvLine(string line)
        {
            // Koristi TextFieldParser za pravilno parsiranje CSV-a sa navodnicima
            using (var parser = new TextFieldParser(new System.IO.StringReader(line)))
            {
                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                parser.HasFieldsEnclosedInQuotes = true;

                if (!parser.EndOfData)
                {
                    return parser.ReadFields();
                }
            }

            return new string[0];
        }
    }
}