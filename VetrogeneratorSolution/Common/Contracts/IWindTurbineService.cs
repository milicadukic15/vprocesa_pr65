using System;
using System.ServiceModel;
using Common.DataModels;
using Common.Exceptions;

namespace Common.Contracts
{
    [ServiceContract]
    public interface IWindTurbineService
    {
        // Pokreće novu sesiju prenosa podataka za određenu turbinu
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMetadata metadata);

        // Šalje jedan uzorak (red iz CSV-a) serveru
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(WindTurbineSample sample);

        // Završava sesiju prenosa
        [OperationContract]
        void EndSession();
    }
}
