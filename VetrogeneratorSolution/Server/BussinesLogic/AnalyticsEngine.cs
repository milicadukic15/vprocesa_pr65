using System;
using System.Configuration;
using Common.DataModels;
using Server.Events;

namespace Server.BusinessLogic
{
    public class AnalyticsEngine
    {
        private readonly double underPerformanceAlpha;
        private readonly double yawMisalignThresholdDeg;
        private readonly double frequencyDeviationAbsHz;
        private readonly double frequencySpikeThresholdHz;

        private double? lastFrequencyHz = null;

        public AnalyticsEngine()
        {
            underPerformanceAlpha = double.Parse(
                ConfigurationManager.AppSettings["UnderPerformanceAlpha"] ?? "0.75"
            );

            yawMisalignThresholdDeg = double.Parse(
                ConfigurationManager.AppSettings["YawMisalignThresholdDeg"] ?? "15"
            );

            frequencyDeviationAbsHz = double.Parse(
                ConfigurationManager.AppSettings["FrequencyDeviationAbsHz"] ?? "0.5"
            );

            frequencySpikeThresholdHz = double.Parse(
                ConfigurationManager.AppSettings["FrequencySpikeThresholdHz"] ?? "0.2"
            );
        }

        public void AnalyzeSample(WindTurbineSample sample)
        {
            CheckUnderPerformance(sample);

            CheckYawMisalignment(sample);

            CheckFrequencyDeviation(sample);

            CheckFrequencySpike(sample);

            lastFrequencyHz = sample.GridFrequencyHz;
        }

        private void CheckUnderPerformance(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.PowerKW) || double.IsNaN(sample.PotentialPowerDefaultKW))
                return;

            if (sample.PotentialPowerDefaultKW < 10)
                return;

            double threshold = underPerformanceAlpha * sample.PotentialPowerDefaultKW;

            if (sample.PowerKW < threshold)
            {
                var warning = new UnderPerformanceWarning(
                    sample.TurbineId,
                    sample.Timestamp,
                    sample.PowerKW,
                    sample.PotentialPowerDefaultKW
                );

                EventPublisher.Instance.RaiseWarning(warning);
            }
        }

        private void CheckYawMisalignment(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.WindDirection) || double.IsNaN(sample.NacellePosition))
                return;

            double diff = Math.Abs(sample.WindDirection - sample.NacellePosition);

            if (diff > 180)
                diff = 360 - diff;

            if (diff > yawMisalignThresholdDeg)
            {
                var warning = new YawMisalignmentWarning(
                    sample.TurbineId,
                    sample.Timestamp,
                    sample.WindDirection,
                    sample.NacellePosition,
                    diff
                );

                EventPublisher.Instance.RaiseWarning(warning);
            }
        }

        private void CheckFrequencyDeviation(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.GridFrequencyHz))
                return;

            const double nominalFrequencyHz = 50.0;
            double deviation = Math.Abs(sample.GridFrequencyHz - nominalFrequencyHz);

            if (deviation > frequencyDeviationAbsHz)
            {
                var warning = new FrequencyDeviationWarning(
                    sample.TurbineId,
                    sample.Timestamp,
                    sample.GridFrequencyHz,
                    nominalFrequencyHz
                );

                EventPublisher.Instance.RaiseWarning(warning);
            }
        }

        private void CheckFrequencySpike(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.GridFrequencyHz))
                return;

            if (!lastFrequencyHz.HasValue)
                return;

            double spike = Math.Abs(sample.GridFrequencyHz - lastFrequencyHz.Value);

            if (spike > frequencySpikeThresholdHz)
            {
                var warning = new FrequencySpikeWarning(
                    sample.TurbineId,
                    sample.Timestamp,
                    lastFrequencyHz.Value,
                    sample.GridFrequencyHz
                );

                EventPublisher.Instance.RaiseWarning(warning);
            }
        }
        public void Reset()
        {
            lastFrequencyHz = null;
        }
    }
}
