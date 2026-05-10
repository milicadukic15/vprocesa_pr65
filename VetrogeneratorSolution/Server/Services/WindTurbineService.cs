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

        private int totalSamplesReceived = 0;
        private int totalSamplesRejected = 0;

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

                fileManager = new FileManager(metadata.TurbineId, metadata.StartTime);

                totalSamplesReceived = 0;
                totalSamplesRejected = 0;

                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Session started successfully. Output: {fileManager.SessionFilePath}");

                Events.EventPublisher.Instance.RaiseTransferStarted(
                    metadata.TurbineId,
                    metadata.StartTime,
                    metadata.FileName
                );
            }
            catch (FaultException)
            {
                throw; 
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

                validator.ValidateSample(sample);

                fileManager.WriteSample(sample);

                totalSamplesReceived++;

                if (totalSamplesReceived % 1000 == 0)
                {
                    Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Processed {totalSamplesReceived} samples...");
                }

                if (totalSamplesReceived % 500 == 0)
                {
                    Events.EventPublisher.Instance.RaiseSampleReceived(
                        currentSession.TurbineId,
                        totalSamplesReceived,
                        0 // Total samples nije poznat serveru
                    );
                }
            }
            catch (Exception ex)
            {
                if (fileManager != null)
                {
                    fileManager.WriteRejectedSample(sample, ex.Message);
                }

                totalSamplesRejected++;

                // Opciono: loguj SAMO svakih 100 rejection-a da ne spamuje konzolu
                if (totalSamplesRejected % 100 == 0)
                {
                    Console.WriteLine($"[WARNING] {totalSamplesRejected} samples rejected so far...");
                }
            }
        }

        public void EndSession()
        {
            try
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Ending session...");
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] Total received: {totalSamplesReceived}, Total rejected: {totalSamplesRejected}");

                // Podignuti TransferCompleted događaj
                Events.EventPublisher.Instance.RaiseTransferCompleted(
                    currentSession?.TurbineId ?? "Unknown",
                    DateTime.Now,
                    totalSamplesReceived,
                    totalSamplesRejected
                );

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
