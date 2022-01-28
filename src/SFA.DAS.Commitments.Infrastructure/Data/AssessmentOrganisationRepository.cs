using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using SFA.DAS.Commitments.Domain.Data;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;

namespace SFA.DAS.Commitments.Infrastructure.Data
{
    public class AssessmentOrganisationRepository : BaseRepository, IAssessmentOrganisationRepository
    {
        private readonly ICommitmentsLogger _logger;

        public AssessmentOrganisationRepository(string connectionString, ICommitmentsLogger logger)
            : base(connectionString, logger.BaseLogger)
        {
            _logger = logger;
        }

        public async Task<string> GetLatestEpaOrgId()
        {
            _logger.Debug("Getting latest EPAOrgId");

            // this makes assumptions about the format of EPAOrgId (which are currently correct)
            return await WithConnection(async connection => await connection.QuerySingleOrDefaultAsync<string>(
                "SELECT MAX([EPAOrgId]) FROM [dbo].[AssessmentOrganisation]",
                commandType: CommandType.Text));
        }

        public async Task Add(IEnumerable<AssessmentOrganisation> assessmentOrganisations)
        {
            _logger.Debug($"Adding {assessmentOrganisations.Count()} assessment organisations, from {assessmentOrganisations.FirstOrDefault()?.EPAOrgId??"N/A"} to {assessmentOrganisations.LastOrDefault()?.EPAOrgId ?? "N/A"}");

            // there are more performant ways of doing this, but we're only dealing with a small number of inserts, so we kiss
            await WithConnection(async connection => await connection.ExecuteAsync(
                "INSERT INTO [dbo].[AssessmentOrganisation] VALUES (@EPAOrgId, @Name)",
                assessmentOrganisations.ToList(),
                commandType: CommandType.Text));
        }
    }
}
