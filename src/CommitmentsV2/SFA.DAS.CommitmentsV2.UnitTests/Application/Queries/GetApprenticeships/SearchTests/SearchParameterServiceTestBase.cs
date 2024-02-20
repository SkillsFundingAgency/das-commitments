using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships.Search.Services.Parameters;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.SearchTests
{
    public abstract class SearchParameterServiceTestBase
    {
        protected static List<Apprenticeship> GetTestApprenticeshipsWithAlerts(ApprenticeshipSearchParameters searchParameters)
        {
            var apprenticeships = new List<Apprenticeship>
            {
                new Apprenticeship
                {
                    FirstName = "A",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New} }
                },
                new Apprenticeship
                {
                    FirstName = "B",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New} }
                },
                new Apprenticeship
                {
                    FirstName = "C",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    ProviderRef = searchParameters.ProviderId.ToString(),
                    EmployerRef = searchParameters.EmployerAccountId.ToString(),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>{new DataLockStatus { IsResolved = false, Status = Status.Fail, EventStatus = EventStatus.New} }
                },
                new Apprenticeship
                {
                    FirstName = "D",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "E",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddMonths(12),
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "F",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    Cohort = new Cohort {AccountLegalEntity = CreateAccountLegalEntity("Employer")},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            if (searchParameters.ProviderId.HasValue)
            {
                AssignProviderToApprenticeships(searchParameters.ProviderId.Value, apprenticeships);
            }

            if (searchParameters.EmployerAccountId.HasValue)
            {
                AssignEmployerToApprenticeships(searchParameters.EmployerAccountId.Value, apprenticeships);
            }


            return apprenticeships;

        }

        protected static void AssignProviderToApprenticeships(long? providerId, IEnumerable<Apprenticeship> apprenticeships)
        {

            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Cohort.ProviderId = providerId.GetValueOrDefault();
                apprenticeship.Cohort.Provider = new Provider
                {
                    UkPrn = providerId.GetValueOrDefault(),
                    Name = "Test Provider",
                    Created = DateTime.Now
                };
            }
        }

        protected static void AssignEmployerToApprenticeships(long? employerAccountId, IEnumerable<Apprenticeship> apprenticeships)
        {
            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Cohort.EmployerAccountId = employerAccountId.GetValueOrDefault();
            }
        }
        protected static AccountLegalEntity CreateAccountLegalEntity(string name)
        {
            var account = new Account(1, "", "", name, DateTime.UtcNow);
            return new AccountLegalEntity(account, 1, 1, "", "", name, OrganisationType.CompaniesHouse, "",
                DateTime.UtcNow);
        }
    }
}