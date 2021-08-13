using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using TrainingProgramme = SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme;

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
                var standard = await _dbContext.Standards.Include(c=>c.FundingPeriods).FirstOrDefaultAsync(c=>c.LarsCode.Equals(standardId) && c.IsLatestVersion);

                if (standard == null)
                {
                    throw new Exception($"The course code {standardId} was not found");
                }
                
                return new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level), ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo, new List<IFundingPeriod>(standard.FundingPeriods));
            }

            var framework = await _dbContext.Frameworks.Include(c=>c.FundingPeriods).FirstOrDefaultAsync(c=>c.Id.Equals(courseCode));
            
            if (framework == null)
            {
                throw new Exception($"The course code {courseCode} was not found");
            }
            
            var frameworkTitle =
                GetTitle(
                    string.Equals(framework.FrameworkName.Trim(), framework.PathwayName.Trim(), StringComparison.OrdinalIgnoreCase)
                        ? framework.FrameworkName
                        : framework.Title, framework.Level) + " (Framework)";
            return new TrainingProgramme(framework.Id, frameworkTitle, ProgrammeType.Framework, framework.EffectiveFrom, framework.EffectiveTo,new List<IFundingPeriod>(framework.FundingPeriods));
                
        }
       
        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }

        public async Task<IEnumerable<TrainingProgramme>> GetAll()
        {
            var frameworksTask = _dbContext.Frameworks.Include(c => c.FundingPeriods).ToListAsync();
            var standardsTask =  _dbContext.Standards.Include(c => c.FundingPeriods).Where(s => s.IsLatestVersion).ToListAsync();

            await Task.WhenAll(frameworksTask, standardsTask);

            var trainingProgrammes = new List<TrainingProgramme>();
            trainingProgrammes.AddRange(frameworksTask.Result.Select(framework=>
                new TrainingProgramme(
                    framework.Id, 
                    GetTitle(string.Equals(framework.FrameworkName.Trim(), framework.PathwayName.Trim(), StringComparison.OrdinalIgnoreCase)
                        ? framework.FrameworkName
                        : framework.Title, framework.Level) + " (Framework)", 
                    ProgrammeType.Framework, 
                    framework.EffectiveFrom, 
                    framework.EffectiveTo,
                    new List<IFundingPeriod>(framework.FundingPeriods))
                )
            );
            trainingProgrammes.AddRange(standardsTask.Result.Select(standard =>
                new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level),
                    ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo,
                    new List<IFundingPeriod>(standard.FundingPeriods))));

            return trainingProgrammes.OrderBy(c=>c.Name);
        }

        public async Task<IEnumerable<TrainingProgramme>> GetAllStandards()
        {
            var standards = await  _dbContext.Standards.Include(c => c.FundingPeriods).Where(s => s.IsLatestVersion).ToListAsync();
            
            var trainingProgrammes = new List<TrainingProgramme>();
            trainingProgrammes.AddRange(standards.Select(standard =>
                new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level),
                    ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo,
                    new List<IFundingPeriod>(standard.FundingPeriods))));

            return trainingProgrammes.OrderBy(c=>c.Name);
        }
    }
}