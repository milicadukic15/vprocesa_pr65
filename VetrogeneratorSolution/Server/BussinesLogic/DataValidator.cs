using System;
using System.ServiceModel;
using Common.DataModels;
using Common.Exceptions;

namespace Server.BusinessLogic
{
    public class DataValidator
    {
        public void ValidateSample(WindTurbineSample sample)
        {
            // Validacija timestamp-a
            if (sample.Timestamp == DateTime.MinValue || sample.Timestamp > DateTime.Now)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Invalid timestamp", "Timestamp", 0),
                    "Timestamp must be a valid past date"
                );
            }

            // Validacija numeričkih vrednosti (moraju biti >= 0 ili razumne vrednosti)

            if (sample.WindSpeed < 0 || sample.WindSpeed > 100)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Wind speed out of range", "WindSpeed", sample.WindSpeed),
                    "Wind speed must be between 0 and 100 m/s"
                );
            }

            if (sample.PowerKW < 0 || sample.PowerKW > 5000)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Power out of range", "PowerKW", sample.PowerKW),
                    "Power must be between 0 and 5000 kW"
                );
            }

            if (sample.PotentialPowerDefaultKW < 0 || sample.PotentialPowerDefaultKW > 5000)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Potential power out of range", "PotentialPowerDefaultKW", sample.PotentialPowerDefaultKW),
                    "Potential power must be between 0 and 5000 kW"
                );
            }

            if (sample.GridFrequencyHz < 40 || sample.GridFrequencyHz > 60)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Grid frequency out of range", "GridFrequencyHz", sample.GridFrequencyHz),
                    "Grid frequency must be between 40 and 60 Hz"
                );
            }

            if (sample.GeneratorRpm < 0 || sample.GeneratorRpm > 3000)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Generator RPM out of range", "GeneratorRpm", sample.GeneratorRpm),
                    "Generator RPM must be between 0 and 3000"
                );
            }

            // Wind direction i Nacelle position (0-360 stepeni)
            if (sample.WindDirection < 0 || sample.WindDirection > 360)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Wind direction out of range", "WindDirection", sample.WindDirection),
                    "Wind direction must be between 0 and 360 degrees"
                );
            }

            if (sample.NacellePosition < 0 || sample.NacellePosition > 360)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Nacelle position out of range", "NacellePosition", sample.NacellePosition),
                    "Nacelle position must be between 0 and 360 degrees"
                );
            }

            // Power factor (obično između -1 i 1)
            if (sample.PowerFactor < -1 || sample.PowerFactor > 1)
            {
                throw new FaultException<ValidationFault>(
                    new ValidationFault("Power factor out of range", "PowerFactor", sample.PowerFactor),
                    "Power factor must be between -1 and 1"
                );
            }
        }
    }
}
