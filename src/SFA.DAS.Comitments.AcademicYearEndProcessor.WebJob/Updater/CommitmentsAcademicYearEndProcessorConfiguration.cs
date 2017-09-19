using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities.DataLock;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Comitments.AcademicYearEndProcessor.WebJob.Updater
{
    public class CommitmentsAcademicYearEndProcessorConfiguration : IConfiguration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }
        public DateTime? CurrentStartTime { get; set; }
        public DataLockErrorCode ExpirableDataLockErrorCodes { get; set; }
    }
}
