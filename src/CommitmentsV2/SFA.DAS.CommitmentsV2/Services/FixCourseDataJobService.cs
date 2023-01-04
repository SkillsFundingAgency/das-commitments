using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Types;
using System;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class FixCourseDataJobService : IFixCourseDataJobService
    {
        private readonly ILogger<FixCourseDataJobService> _logger;
        private readonly Lazy<ProviderCommitmentsDbContext> _db;
        private readonly ITrainingProgrammeLookup _trainingProgrammeLookup;

        public FixCourseDataJobService(
            ILogger<FixCourseDataJobService> logger,
            Lazy<ProviderCommitmentsDbContext> db,
            ITrainingProgrammeLookup trainingProgrammeLookup
            )
        {
            _logger = logger;
            _db = db;
            _trainingProgrammeLookup = trainingProgrammeLookup;
        }

        public async Task RunUpdate()
        {
            _logger.LogInformation("run query");

            var apprenticeships = await _db.Value.Apprenticeships
                                .FromSqlRaw(
                                    "SELECT *" +
                                    "FROM[dbo].[Apprenticeship] A" +
                                    "LEFT JOIN[Standard] S ON S.LarsCode = A.TrainingCode AND S.StandardUId = A.StandardUId" +
                                    "LEFT JOIN[Standard] SD ON SD.StandardUId = A.StandardUId" +
                                    "WHERE A.TrainingType != 1 AND A.StandardUId IS NOT NULL AND S.StandardUId IS NULL AND A.CreatedOn > '2019-01-01' AND A.IsApproved = 1" +
                                    "AND A.StopDate is null" +
                                    "ORDER BY A.Id, A.TrainingCode, CreatedOn"
                                    )
                                .ToListAsync();

            foreach (var app in apprenticeships)
            {
                var training = await _trainingProgrammeLookup.GetTrainingProgramme(app.CourseCode);
                _logger.LogInformation($"Updating course for apprenticeship {app.Id} from training code {app.CourseCode} to {training.CourseCode}");
                app.UpdateCourse(Party.Employer, training.CourseCode, training.Name, training.ProgrammeType, new UserInfo() { UserDisplayName = "webjob", UserEmail = "web@job.com", UserId = "1"});
            }
        }
    }
}