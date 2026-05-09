using System;
using System.ServiceModel;
using Common.Contracts;
using Common.DataModels;
using Common.Exceptions;
using Server.BusinessLogic;

namespace Server.Services
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public class WindTurbineService : IWindTurbineService, IDisposable
    {
        private FileManager fileManager;
        private DataValidator validator;
        private SessionMetadata currentSession;
        private bool disposed = false;

        public WindTurbineService()
        {
            validator = new DataValidator();
        }

        public void StartSession(SessionMetadata metadata)
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Starting session for Turbine: {metadata.TurbineId}");

                // Validacija metadata
                if (string.IsNullOrWhiteSpace(metadata.TurbineId))
                {
                    throw new FaultException<ValidationFault>(
                        new ValidationFault("TurbineId cannot be empty", "TurbineId", 0),
                        "Invalid session metadata"
                    );
                }

                currentSession = metadata;

                // Kreiranje FileManager-a za ovu sesiju
                fileManager = new FileManager(metadata.TurbineId, metadata.StartTime);

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session started successfully. Output: {fileManager.SessionFilePath}");
            }
            catch (FaultException)
            {
                throw; // Re-throw WCF fault exceptions
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] StartSession failed: {ex.Message}");
                throw new FaultException<DataFormatFault>(
                    new DataFormatFault($"Failed to start session: {ex.Message}", "Session", ""),
                    "Session initialization error"
                );
            }
        }

        public void PushSample(WindTurbineSample sample)
        {
            try
            {
                if (fileManager == null)
                {
                    throw new FaultException("No active session. Call StartSession first.");
                }

                // Validacija uzorka
                validator.ValidateSample(sample);

                // Snimanje u fajl
                fileManager.WriteSample(sample);

                // Console feedback (opciono, ne loguj svaki red jer će biti previše)
                if (sample.RowIndex % 100 == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Processed {sample.RowIndex} samples...");
                }
            }
            catch (FaultException)
            {
                throw; // Re-throw WCF faults
            }
            catch (Exception ex)
            {
                // Logovanje rejected uzorka
                if (fileManager != null)
                {
                    fileManager.WriteRejectedSample(sample, ex.Message);
                }

                Console.WriteLine($"[WARNING] Sample rejected (Row {sample.RowIndex}): {ex.Message}");

                // Ne throw-uj exception - nastavi sa sledećim uzorkom
                // Ovo omogućava da se prenos nastavi čak i ako neki uzorci nisu validni
            }
        }

        public void EndSession()
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Ending session...");

                Dispose();

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session ended successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] EndSession failed: {ex.Message}");
            }
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
                    if (fileManager != null)
                    {
                        fileManager.Dispose();
                        fileManager = null;
                    }
                }
                disposed = true;
            }
        }

        ~WindTurbineService()
        {
            Dispose(false);
        }
    }
}
