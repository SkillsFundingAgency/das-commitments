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
                var standard = await _dbContext.Standards.Include(c => c.FundingPeriods).FirstOrDefaultAsync(c => c.LarsCode.Equals(standardId) && c.IsLatestVersion);

                if (standard == null)
                {
                    throw new Exception($"The course code {standardId} was not found");
                }

                return new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level), ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo, new List<IFundingPeriod>(standard.FundingPeriods));
            }

            var framework = await _dbContext.Frameworks.Include(c => c.FundingPeriods).FirstOrDefaultAsync(c => c.Id.Equals(courseCode));

            if (framework == null)
            {
                throw new Exception($"The course code {courseCode} was not found");
            }

            var frameworkTitle =
                GetTitle(
                    string.Equals(framework.FrameworkName.Trim(), framework.PathwayName.Trim(), StringComparison.OrdinalIgnoreCase)
                        ? framework.FrameworkName
                        : framework.Title, framework.Level) + " (Framework)";

            return new TrainingProgramme(framework.Id, frameworkTitle, ProgrammeType.Framework, framework.EffectiveFrom, framework.EffectiveTo, new List<IFundingPeriod>(framework.FundingPeriods));
        }

        public async Task<TrainingProgramme> GetCalculatedTrainingProgrammeVersion(string courseCode, DateTime startDate)
        {

            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return null;
            }

            if (!int.TryParse(courseCode, out var standardId))
            {
                return null;
            }

            var standardVersions = await _dbContext.Standards.AsNoTracking().Include(c => c.FundingPeriods).Where(s => s.LarsCode == standardId)
                .OrderBy(s => s.VersionMajor).ThenBy(t => t.VersionMinor).ToListAsync();

            TrainingProgramme trainingProgramme = null;

            if (standardVersions.Count() == 0)
            {
                return trainingProgramme;
            }

            // Overwrite EffectiveFrom of all versions to 1st of each month so that if a version starts in the same month
            // First version doesn't get it's effective from overwritten as that won't have an overlap
            // Last version effective to doesn't matter as it should be null
            // e.g.
            // 1.0  Effective From 9/12/2019 Effective To 14/7/2020
            // 1.1  Effective From 15/7/2020 Effective To 19/10/2020
            // 1.2  Effective From 20/10/2020  Effective To Null

            // Becomes
            // 1.0  Effective From 9/12/2019 Effective To 31/7/2020
            // 1.1  Effective From 1/7/2020 Effective To 31/10/2020
            // 1.2  Effective From 1/10/2020  Effective To Null
            
            var first = true;
            foreach (var version in standardVersions)
            {
                if (!first && version.EffectiveFrom.HasValue)
                {
                    version.EffectiveFrom = new DateTime(version.EffectiveFrom.Value.Year, version.EffectiveFrom.Value.Month, 1);
                }

                if (version.EffectiveTo.HasValue)
                {
                    var daysInMonth = DateTime.DaysInMonth(version.EffectiveTo.Value.Year, version.EffectiveTo.Value.Month);
                    version.EffectiveTo = new DateTime(version.EffectiveTo.Value.Year, version.EffectiveTo.Value.Month, daysInMonth);
                }

                first = false;
            }

            // Given the above resetting
            // If an apprentice start date is then 29th October 2020
            // 29/10/2020 is > 1/7/2020  and it's < 31/10/2020 so it initially creates a 1.1 Training Programme
            // 29/10/2020 is > 1/10/2020 and Effective To Is null, so then ovewrites with a 1.2 Training Programme
            Standard selectedVersion = standardVersions.Last();
            foreach (var version in standardVersions)
            {
                if (startDate >= version.EffectiveFrom && (version.EffectiveTo.HasValue == false || startDate <= version.EffectiveTo.Value)) 
                {
                    selectedVersion = version;
                }
            }

            return new TrainingProgramme(selectedVersion.LarsCode.ToString(), GetTitle(selectedVersion.Title, selectedVersion.Level), selectedVersion.Version, selectedVersion.StandardUId,
                        ProgrammeType.Standard, selectedVersion.StandardPageUrl, selectedVersion.EffectiveFrom, selectedVersion.EffectiveTo, new List<IFundingPeriod>(selectedVersion.FundingPeriods), selectedVersion.Options?.Select(o => o.Option).ToList());
        }

        public async Task<TrainingProgramme> GetTrainingProgrammeVersionByStandardUId(string standardUId)
        {
            var standard = await _dbContext.Standards.Include(c => c.Options).Include(c => c.FundingPeriods).FirstOrDefaultAsync(c => c.StandardUId.Equals(standardUId));

            if (standard == null)
            {
                throw new Exception($"The standard {standardUId} was not found");
            }

            return new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level), standard.Version, standard.StandardUId, ProgrammeType.Standard, standard.StandardPageUrl, 
                standard.EffectiveFrom, standard.EffectiveTo, new List<IFundingPeriod>(standard.FundingPeriods), standard.Options?.Select(o => o.Option).ToList());
        }
       
        private static string GetTitle(string title, int level)
        {
            return $"{title}, Level: {level}";
        }

        public async Task<IEnumerable<TrainingProgramme>> GetAll()
        {
            var frameworksTask = _dbContext.Frameworks.Include(c => c.FundingPeriods).ToListAsync();
            var standardsTask = _dbContext.Standards.Include(c => c.FundingPeriods).Where(s => s.IsLatestVersion).ToListAsync();

            await Task.WhenAll(frameworksTask, standardsTask);

            var trainingProgrammes = new List<TrainingProgramme>();
            trainingProgrammes.AddRange(frameworksTask.Result.Select(framework =>
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

            return trainingProgrammes.OrderBy(c => c.Name);
        }

        public async Task<IEnumerable<TrainingProgramme>> GetAllStandards()
        {
            var standards = await _dbContext.Standards.Include(c => c.FundingPeriods).Where(s => s.IsLatestVersion).ToListAsync();

            var trainingProgrammes = new List<TrainingProgramme>();
            trainingProgrammes.AddRange(standards.Select(standard =>
                new TrainingProgramme(standard.LarsCode.ToString(), GetTitle(standard.Title, standard.Level),
                    ProgrammeType.Standard, standard.EffectiveFrom, standard.EffectiveTo,
                    new List<IFundingPeriod>(standard.FundingPeriods))));

            return trainingProgrammes.OrderBy(c => c.Name);
        }
    }
}