using System;
using Common.DataModels;

namespace Server.Events
{
    /// Implementira Publish-Subscribe pattern
    public class EventPublisher
    {
        // Singleton instance
        private static EventPublisher instance;
        private static readonly object lockObj = new object();

        public static EventPublisher Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObj)
                    {
                        if (instance == null)
                        {
                            instance = new EventPublisher();
                        }
                    }
                }
                return instance;
            }
        }

        private EventPublisher()
        {
            // Private constructor za Singleton
        }

        // Delegate za događaje
        public delegate void TransferStartedEventHandler(object sender, TransferStartedEventArgs e);
        public delegate void SampleReceivedEventHandler(object sender, SampleReceivedEventArgs e);
        public delegate void TransferCompletedEventHandler(object sender, TransferCompletedEventArgs e);
        public delegate void WarningRaisedEventHandler(object sender, WarningEventArgs e);

        // Događaji
        public event TransferStartedEventHandler OnTransferStarted;
        public event SampleReceivedEventHandler OnSampleReceived;
        public event TransferCompletedEventHandler OnTransferCompleted;
        public event WarningRaisedEventHandler OnWarningRaised;

        public void RaiseTransferStarted(string turbineId, DateTime startTime, string fileName)
        {
            OnTransferStarted?.Invoke(this, new TransferStartedEventArgs(turbineId, startTime, fileName));
        }

        public void RaiseSampleReceived(string turbineId, int sampleCount, int totalSamples)
        {
            OnSampleReceived?.Invoke(this, new SampleReceivedEventArgs(turbineId, sampleCount, totalSamples));
        }

        public void RaiseTransferCompleted(string turbineId, DateTime endTime, int totalReceived, int totalRejected)
        {
            OnTransferCompleted?.Invoke(this, new TransferCompletedEventArgs(turbineId, endTime, totalReceived, totalRejected));
        }

        public void RaiseWarning(WarningEventArgs warning)
        {
            OnWarningRaised?.Invoke(this, warning);
        }
    }
}
