namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler
    {
        private class EmployerSummaries : List<EmployerSummary>
        {
            internal bool ContainsKey(string agreementId)
            {
                return this.Any(x => x.AgreementId == agreementId);
            }

            internal EmployerSummary GetValueOrDefault(string agreementId)
            {
                return this.First(x => x.AgreementId == agreementId);
            }
        }

        private class EmployerSummary
        {
            public EmployerSummary(string agreementId, long? legalEntityId, bool? isLevy, string name, bool? isSigned, string accountLegalEntityId)
            {
                AgreementId = agreementId;
                LegalEntityId = legalEntityId;
                IsLevy = isLevy;
                Name = name;
                IsSigned = isSigned;
                AccountLegalEntityId = accountLegalEntityId;
                HasPermissionToCreateCohort = false;
            }

            public string AgreementId { get; set; }
            public long? LegalEntityId { get; set; }
            public string AccountLegalEntityId { get; set; }
            public bool? IsLevy { get; set; }
            public string Name { get; set; }
            public bool? IsSigned { get; set; }
            public bool? HasPermissionToCreateCohort { get; set; }
        }
    }
}
