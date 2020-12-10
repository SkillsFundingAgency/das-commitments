using System;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Services
{
    
    public class TrainingProgrammeLookup : ITrainingProgrammeLookup
    {
        private readonly IProviderCommitmentsDbContext _dbContext;
        

        public TrainingProgrammeLookup(IProviderCommitmentsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<TrainingProgramme> GetTrainingProgramme(string courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return null;
            }
            
            if (int.TryParse(courseCode, out var standardId))
            {
                var standard = await _dbContext.Standards.FindAsync(standardId);

                if (standard == null)
                {
                    throw new Exception($"The course code {standardId} was not found");
                }
                
                return new TrainingProgramme(standard.Id.ToString(),GetTitle(standard.Title, standard.Level) + " (Standard)",ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo);
            }

            var framework = await _dbContext.Frameworks.FindAsync(courseCode);
            
            if (framework == null)
            {
                throw new Exception($"The course code {courseCode} was not found");
            }
            
            var frameworkTitle =
                GetTitle(
                    string.Equals(framework.FrameworkName.Trim(), framework.PathwayName.Trim(), StringComparison.OrdinalIgnoreCase)
                        ? framework.FrameworkName
                        : framework.Title, framework.Level);
            return new TrainingProgramme(framework.Id, frameworkTitle, ProgrammeType.Framework, framework.EffectiveFrom, framework.EffectiveTo);
                
        }
       
        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }
    }
}