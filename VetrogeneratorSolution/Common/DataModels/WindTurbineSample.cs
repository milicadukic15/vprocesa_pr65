using System;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    [DataContract]
    public class WindTurbineSample
    {
        [DataMember]
        public DateTime Timestamp { get; set; }

        [DataMember]
        public double WindSpeed { get; set; }

        [DataMember]
        public double WindDirection { get; set; }

        [DataMember]
        public double NacellePosition { get; set; }

        [DataMember]
        public double PowerKW { get; set; }

        [DataMember]
        public double PotentialPowerDefaultKW { get; set; }

        [DataMember]
        public double PowerFactor { get; set; }

        [DataMember]
        public double ReactivePowerKvar { get; set; }

        [DataMember]
        public double GridFrequencyHz { get; set; }

        [DataMember]
        public double GeneratorRpm { get; set; }

        // Dodatna polja za tracking

        [DataMember]
        public int RowIndex { get; set; }

        [DataMember]
        public string TurbineId { get; set; }

        public WindTurbineSample()
        {
        }
    }
}
