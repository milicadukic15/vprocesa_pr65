using System;
using System.IO;
using System.Configuration;
using System.Globalization;
using Common.DataModels;

namespace Server.BusinessLogic
{
    public class FileManager : IDisposable
    {
        private StreamWriter sessionWriter;
        private StreamWriter rejectsWriter;
        private bool disposed = false;

        public string SessionFilePath { get; private set; }
        public string RejectsFilePath { get; private set; }

        public FileManager(string turbineId, DateTime startTime)
        {
            // Kreiranje strukture: Data/<TurbineID>/<YYYY-MM-DD>/
            string basePath = ConfigurationManager.AppSettings["DataStoragePath"] ?? "Data";
            string dateFolder = startTime.ToString("yyyy-MM-dd");
            string outputDirectory = Path.Combine(basePath, turbineId, dateFolder);

            // Kreiranje direktorijuma ako ne postoji
            Directory.CreateDirectory(outputDirectory);

            // Putanje do fajlova
            SessionFilePath = Path.Combine(outputDirectory, "session.csv");
            RejectsFilePath = Path.Combine(outputDirectory, "rejects.csv");

            // Otvaranje stream-ova
            sessionWriter = new StreamWriter(SessionFilePath, append: false);
            rejectsWriter = new StreamWriter(RejectsFilePath, append: false);

            // Pisanje header-a
            WriteSessionHeader();
            WriteRejectsHeader();
        }

        private void WriteSessionHeader()
        {
            sessionWriter.WriteLine("Timestamp,WindSpeed,WindDirection,NacellePosition,PowerKW,PotentialPowerDefaultKW,PowerFactor,ReactivePowerKvar,GridFrequencyHz,GeneratorRpm,RowIndex,TurbineId");
        }

        private void WriteRejectsHeader()
        {
            rejectsWriter.WriteLine("Timestamp,RowIndex,TurbineId,Reason,OriginalLine");
        }

        public void WriteSample(WindTurbineSample sample)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(FileManager));

            // Format sa InvariantCulture (decimalna tačka)
            string line = string.Format(CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11}",
                sample.Timestamp,
                sample.WindSpeed,
                sample.WindDirection,
                sample.NacellePosition,
                sample.PowerKW,
                sample.PotentialPowerDefaultKW,
                sample.PowerFactor,
                sample.ReactivePowerKvar,
                sample.GridFrequencyHz,
                sample.GeneratorRpm,
                sample.RowIndex,
                sample.TurbineId
            );

            sessionWriter.WriteLine(line);
            sessionWriter.Flush(); // Flush posle svakog reda za sigurnost
        }

        public void WriteRejectedSample(WindTurbineSample sample, string reason)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(FileManager));

            string originalLine = $"Row {sample.RowIndex}, TurbineId={sample.TurbineId}";

            string line = string.Format(CultureInfo.InvariantCulture,
                "{0:yyyy-MM-dd HH:mm:ss},{1},{2},{3},{4}",
                DateTime.Now,
                sample.RowIndex,
                sample.TurbineId,
                reason.Replace(",", ";"), // Escape commas
                originalLine
            );

            rejectsWriter.WriteLine(line);
            rejectsWriter.Flush();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    // Dispose managed resources
                    if (sessionWriter != null)
                    {
                        sessionWriter.Flush();
                        sessionWriter.Close();
                        sessionWriter.Dispose();
                        sessionWriter = null;
                    }

                    if (rejectsWriter != null)
                    {
                        rejectsWriter.Flush();
                        rejectsWriter.Close();
                        rejectsWriter.Dispose();
                        rejectsWriter = null;
                    }
                }
                disposed = true;
            }
        }

        ~FileManager()
        {
            Dispose(false);
        }
    }
}
