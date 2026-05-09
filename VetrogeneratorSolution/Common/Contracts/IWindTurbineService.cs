using System;
using System.ServiceModel;
using Common.DataModels;
using Common.Exceptions;

namespace Common.Contracts
{
    [ServiceContract]
    public interface IWindTurbineService
    {
        /// <summary>
        /// Pokreće novu sesiju prenosa podataka za određenu turbinu
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void StartSession(SessionMetadata metadata);

        /// <summary>
        /// Šalje jedan uzorak (red iz CSV-a) serveru
        /// </summary>
        [OperationContract]
        [FaultContract(typeof(DataFormatFault))]
        [FaultContract(typeof(ValidationFault))]
        void PushSample(WindTurbineSample sample);

        /// <summary>
        /// Završava sesiju prenosa
        /// </summary>
        [OperationContract]
        void EndSession();
    }
}
