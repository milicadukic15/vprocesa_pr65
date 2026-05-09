using System.Runtime.Serialization;

namespace Common.Exceptions
{
    [DataContract]
    public class ValidationFault
    {
        [DataMember]
        public string Message { get; set; }

        [DataMember]
        public string FieldName { get; set; }

        [DataMember]
        public double ActualValue { get; set; }

        public ValidationFault()
        {
        }

        public ValidationFault(string message, string fieldName, double actualValue)
        {
            Message = message;
            FieldName = fieldName;
            ActualValue = actualValue;
        }
    }
}