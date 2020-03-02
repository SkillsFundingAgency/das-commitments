using System;
using System.Collections.Generic;
using SFA.DAS.CommitmentsV2.Application.Queries.GetApprenticeships;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Queries.GetApprenticeships.GetApprenticeshipsHandlerTests
{
    public abstract class GetApprenticeshipsHandlerTestBase
    {
        protected static List<Apprenticeship> GetTestApprenticeshipsWithAlerts(GetApprenticeshipsQuery query)
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            return apprenticeships;
        }

         protected static List<Apprenticeship> GetTestApprenticeshipsWithoutAlerts(GetApprenticeshipsQuery query)
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
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "B",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "C",
                    LastName = "Zog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                },
                new Apprenticeship
                {
                    FirstName = "D",
                    LastName = "Fog",
                    Uln = "Uln",
                    CourseName = "Course",
                    StartDate = DateTime.UtcNow,
                    ProviderRef = query.ProviderId.ToString(),
                    Cohort = new Cohort {LegalEntityName = "Employer"},
                    ApprenticeshipUpdate = new List<ApprenticeshipUpdate>(),
                    DataLockStatus = new List<DataLockStatus>()
                }
            };

            AssignProviderToApprenticeships(query.ProviderId, apprenticeships);

            return apprenticeships;
        }

        protected static void AssignProviderToApprenticeships(long providerId, IEnumerable<Apprenticeship> apprenticeships)
        {
            foreach (var apprenticeship in apprenticeships)
            {
                apprenticeship.Cohort.ProviderId = providerId;
            }
        }
    }
}