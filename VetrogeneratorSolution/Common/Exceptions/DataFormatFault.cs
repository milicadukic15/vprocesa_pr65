using System.Runtime.Serialization;

namespace Common.Exceptions
{
    [DataContract]
    public class DataFormatFault
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public string RawValue { get; set; }

        public DataFormatFault()
        {
        }

        public DataFormatFault(string message, string fieldName, string rawValue)
        {
            Message = message;
            FieldName = fieldName;
            RawValue = rawValue;
        }
    }
}
