using SFA.DAS.Commitments.Api.IntegrationTests.DatabaseSetup.Entities;
using SFA.DAS.Commitments.Api.Types;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;
using SFA.DAS.Commitments.Api.Types.Commitment.Types;
using OrganisationType = SFA.DAS.Common.Domain.Types.OrganisationType;

namespace SFA.DAS.Commitments.Api.IntegrationTests.Helpers
{
    public static class TestEntities
    {
        public static DbSetupCommitment GetDbSetupCommitment()
        {
            return new DbSetupCommitment
            {
                //todo: this is just the non-nullable for now
                Reference = "Reference",
                LegalEntityId = "LegalId",
                LegalEntityName = "LegalName",
                LegalEntityAddress = "LegalAddress",
                LegalEntityOrganisationType = OrganisationType.CompaniesHouse,
                EditStatus = EditStatus.EmployerOnly,
                LastUpdatedByEmployerName = "LastUpdatedByEmployerName", // why is this non-null?
                LastUpdatedByEmployerEmail = "LastUpdatedByEmployerEmail@example.com"
            };
        }

        public static DbSetupApprenticeship GetDbSetupApprenticeship(long commitmentId, string firstName, string lastName)
        {
            return new DbSetupApprenticeship
            {
                CommitmentId = commitmentId,
                FirstName = firstName,
                LastName = lastName,
                //todo: other nullable fields
                AgreementStatus = AgreementStatus.EmployerAgreed,
                PaymentStatus = PaymentStatus.Active
            };
        }

        //public static T Clone<T>(T source)
        //{
        //    var serialized = JsonConvert.SerializeObject(source);
        //    return JsonConvert.DeserializeObject<T>(serialized);
        //}
    }
}
