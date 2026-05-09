using System;
using System.ServiceModel;
using Common.Contracts;
using Common.DataModels;

namespace Client.Proxy
{
    public class ServiceProxy : IDisposable
    {
        private ChannelFactory<IWindTurbineService> factory;
        private IWindTurbineService channel;
        private bool disposed = false;

        public ServiceProxy()
        {
            try
            {
                // Kreiranje ChannelFactory-ja sa konfiguracijom iz App.config
                factory = new ChannelFactory<IWindTurbineService>("WindTurbineServiceEndpoint");
                channel = factory.CreateChannel();

                Console.WriteLine("[INFO] Connection to server established.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to connect to server: {ex.Message}");
                throw;
            }
        }

        public void StartSession(SessionMetadata metadata)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ServiceProxy));

            try
            {
                channel.StartSession(metadata);
            }
            catch (FaultException<Common.Exceptions.ValidationFault> ex)
            {
                Console.WriteLine($"[VALIDATION ERROR] {ex.Detail.Message} (Field: {ex.Detail.FieldName})");
                throw;
            }
            catch (FaultException<Common.Exceptions.DataFormatFault> ex)
            {
                Console.WriteLine($"[FORMAT ERROR] {ex.Detail.Message} (Field: {ex.Detail.FieldName})");
                throw;
            }
            catch (FaultException ex)
            {
                Console.WriteLine($"[SERVER ERROR] {ex.Message}");
                throw;
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine($"[COMMUNICATION ERROR] {ex.Message}");
                throw;
            }
        }

        public void PushSample(WindTurbineSample sample)
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ServiceProxy));

            try
            {
                channel.PushSample(sample);
            }
            catch (FaultException<Common.Exceptions.ValidationFault> ex)
            {
                // Validation errors se loguju ali ne prekidaju prenos
                // Server će odbaciti uzorak u rejects.csv
            }
            catch (FaultException<Common.Exceptions.DataFormatFault> ex)
            {
                // Format errors se loguju ali ne prekidaju prenos
            }
            catch (CommunicationException ex)
            {
                Console.WriteLine($"[COMMUNICATION ERROR] Failed to send sample (Row {sample.RowIndex}): {ex.Message}");
                throw;
            }
        }

        public void EndSession()
        {
            if (disposed)
                throw new ObjectDisposedException(nameof(ServiceProxy));

            try
            {
                channel.EndSession();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[WARNING] Error ending session: {ex.Message}");
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
                    try
                    {
                        if (channel != null)
                        {
                            var communicationObject = channel as ICommunicationObject;
                            if (communicationObject != null)
                            {
                                if (communicationObject.State == CommunicationState.Faulted)
                                {
                                    communicationObject.Abort();
                                }
                                else
                                {
                                    communicationObject.Close();
                                }
                            }
                        }

                        if (factory != null)
                        {
                            if (factory.State == CommunicationState.Faulted)
                            {
                                factory.Abort();
                            }
                            else
                            {
                                factory.Close();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WARNING] Error disposing proxy: {ex.Message}");
                    }
                    finally
                    {
                        channel = null;
                        factory = null;
                    }
                }
                disposed = true;
            }
        }

        ~ServiceProxy()
        {
            Dispose(false);
        }
    }
}
