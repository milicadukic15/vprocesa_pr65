using System;
using System.ServiceModel;
using Common.DataModels;
using Common.Exceptions;

namespace Common.Contracts
{
    [ServiceContract(SessionMode = SessionMode.Required)]
    public interface IWindTurbineService
    {
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMetadata metadata);

        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(WindTurbineSample sample);

        [OperationContract]
        void EndSession();
    }
}
