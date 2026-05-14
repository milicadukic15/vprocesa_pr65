using System;

namespace Common.DataModels
{
    /// Base class za sve warning događaje
    public class WarningEventArgs : EventArgs
    {
        public string TurbineId { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public string WarningType { get; set; }

        public WarningEventArgs(string turbineId, DateTime timestamp, string message, string warningType)
        {
            TurbineId = turbineId;
            Timestamp = timestamp;
            Message = message;
            WarningType = warningType;
        }
    }

    // Transfer started event arguments
    public class TransferStartedEventArgs : EventArgs
    {
        public string TurbineId { get; set; }
        public DateTime StartTime { get; set; }
        public string FileName { get; set; }

        public TransferStartedEventArgs(string turbineId, DateTime startTime, string fileName)
        {
            TurbineId = turbineId;
            StartTime = startTime;
            FileName = fileName;
        }
    }

    /// Sample received event arguments
    public class SampleReceivedEventArgs : EventArgs
    {
        public int SampleCount { get; set; }
        public int TotalSamples { get; set; }
        public string TurbineId { get; set; }

        public SampleReceivedEventArgs(string turbineId, int sampleCount, int totalSamples)
        {
            TurbineId = turbineId;
            SampleCount = sampleCount;
            TotalSamples = totalSamples;
        }
    }

    /// Transfer completed event arguments
    public class TransferCompletedEventArgs : EventArgs
    {
        public string TurbineId { get; set; }
        public DateTime EndTime { get; set; }
        public int TotalSamplesReceived { get; set; }
        public int TotalSamplesRejected { get; set; }

        public TransferCompletedEventArgs(string turbineId, DateTime endTime, int totalReceived, int totalRejected)
        {
            TurbineId = turbineId;
            EndTime = endTime;
            TotalSamplesReceived = totalReceived;
            TotalSamplesRejected = totalRejected;
        }
    }


    public class UnderPerformanceWarning : WarningEventArgs
    {
        public double ActualPowerKW { get; set; }
        public double PotentialPowerKW { get; set; }
        public double PerformanceRatio { get; set; }

        public UnderPerformanceWarning(string turbineId, DateTime timestamp, double actualPower, double potentialPower)
            : base(turbineId, timestamp,
                  $"Under-performance detected: {actualPower:F2} kW vs potential {potentialPower:F2} kW",
                  "UnderPerformance")
        {
            ActualPowerKW = actualPower;
            PotentialPowerKW = potentialPower;
            PerformanceRatio = potentialPower > 0 ? actualPower / potentialPower : 0;
        }
    }

    public class YawMisalignmentWarning : WarningEventArgs
    {
        public double WindDirection { get; set; }
        public double NacellePosition { get; set; }
        public double MisalignmentDegrees { get; set; }

        public YawMisalignmentWarning(string turbineId, DateTime timestamp, double windDir, double nacellePos, double misalignment)
            : base(turbineId, timestamp,
                  $"Yaw misalignment: {misalignment:F1}° (Wind: {windDir:F1}°, Nacelle: {nacellePos:F1}°)",
                  "YawMisalignment")
        {
            WindDirection = windDir;
            NacellePosition = nacellePos;
            MisalignmentDegrees = misalignment;
        }
    }

    public class FrequencyDeviationWarning : WarningEventArgs
    {
        public double ActualFrequencyHz { get; set; }
        public double NominalFrequencyHz { get; set; }
        public double DeviationHz { get; set; }

        public FrequencyDeviationWarning(string turbineId, DateTime timestamp, double actualFreq, double nominalFreq)
            : base(turbineId, timestamp,
                  $"Frequency deviation: {actualFreq:F2} Hz (nominal: {nominalFreq} Hz)",
                  "FrequencyDeviation")
        {
            ActualFrequencyHz = actualFreq;
            NominalFrequencyHz = nominalFreq;
            DeviationHz = Math.Abs(actualFreq - nominalFreq);
        }
    }

    public class FrequencySpikeWarning : WarningEventArgs
    {
        public double PreviousFrequencyHz { get; set; }
        public double CurrentFrequencyHz { get; set; }
        public double SpikeHz { get; set; }

        public FrequencySpikeWarning(string turbineId, DateTime timestamp, double prevFreq, double currentFreq)
            : base(turbineId, timestamp,
                  $"Frequency spike detected: Δf = {Math.Abs(currentFreq - prevFreq):F2} Hz",
                  "FrequencySpike")
        {
            PreviousFrequencyHz = prevFreq;
            CurrentFrequencyHz = currentFreq;
            SpikeHz = Math.Abs(currentFreq - prevFreq);
        }
    }
}
