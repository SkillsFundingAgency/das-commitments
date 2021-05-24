namespace SFA.DAS.CommitmentsV2.Domain.Exceptions
{
    public class DomainError
    {
        public DomainError(string propertyName, string errorMessage)
        {
            PropertyName = propertyName;
            ErrorMessage = errorMessage;
        }

        public string PropertyName { get; }
        public string ErrorMessage { get; }
    }

    public class BulkUploadDomainError : DomainError
    {
        public BulkUploadDomainError(string empName, string cohortRef, string uln, string name, string error)
            :base("ProviderId", error)
        {
            EmployerName = empName;
            CohortRef = cohortRef;
            ULN = uln;
            Name = name;
        }

        public string EmployerName { get; set; }

        public string CohortRef { get; set; }

        public string ULN { get; set; }
        public string Name { get; set; }
    }
}
