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
            // Učitavanje pragova iz App.config
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

        /// Provera da li turbina proizvodi manje od alpha * potential power
        private void CheckUnderPerformance(WindTurbineSample sample)
        {
            // Provera da li postoje validne vrednosti (nisu NaN)
            if (double.IsNaN(sample.PowerKW) || double.IsNaN(sample.PotentialPowerDefaultKW))
                return;

            // Ignorisati slučajeve kada je potencijalna snaga vrlo mala (turbina stoji)
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

        /// Provera yaw misalignment-a (razlika između wind direction i nacelle position)
        private void CheckYawMisalignment(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.WindDirection) || double.IsNaN(sample.NacellePosition))
                return;

            // Računanje najkraćeg ugaonog odstojanja (uzimajući u obzir 0°/360° wrap)
            double diff = Math.Abs(sample.WindDirection - sample.NacellePosition);

            // Normalizacija na [0, 180] range
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

        /// Provera odstupanja frekvencije od nominalne (50 Hz)
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

        /// Provera nagle promene frekvencije između dva uzastopna uzorka
        private void CheckFrequencySpike(WindTurbineSample sample)
        {
            if (double.IsNaN(sample.GridFrequencyHz))
                return;

            // Preskočiti prvi uzorak (nema prethodnu frekvenciju za poređenje)
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
