using System;
using System.Runtime.Serialization;

namespace Common.DataModels
{
    [DataContract]
    public class SessionMetadata
    {
        [DataMember]
        public string TurbineId { get; set; }

        [DataMember]
        public DateTime StartTime { get; set; }

        [DataMember]
        public string FileName { get; set; }

        public SessionMetadata()
        {
        }

        public SessionMetadata(string turbineId, DateTime startTime, string fileName)
        {
            TurbineId = turbineId;
            StartTime = startTime;
            FileName = fileName;
        }
    }
}
