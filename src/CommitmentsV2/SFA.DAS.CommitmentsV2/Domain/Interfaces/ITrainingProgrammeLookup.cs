using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface ITrainingProgrammeLookup
    {
        Task<TrainingProgramme> GetTrainingProgramme(string courseCode);
        Task<TrainingProgramme> GetTrainingProgrammeVersion(int courseCode, DateTime startDate);
        Task<IEnumerable<TrainingProgramme>> GetAll();
        Task<IEnumerable<TrainingProgramme>> GetAllStandards();

    }
}