﻿using System;
using System.Collections.Generic;
using System.Linq;

using FluentAssertions;
using NUnit.Framework;

using SFA.DAS.Commitments.Api.Types.Apprenticeship;
using SFA.DAS.Commitments.Api.Types.Apprenticeship.Types;

namespace SFA.DAS.Commitments.Application.UnitTests.Service.ApprenticeshipFilterService
{
    using SFA.DAS.Commitments.Application.Services;

    [TestFixture]
    public class WhenFilterApprenticeships
    {
        private ApprenticeshipFilterService _sut;

        private List<Apprenticeship> _apprenticeships;

        [SetUp]
        public void SetUp()
        {
            _apprenticeships = new List<Apprenticeship>
                                   {
                                       new Apprenticeship
                                           {
                                               Id = 006,
                                               FirstName = "Live",
                                               PaymentStatus = PaymentStatus.Active,
                                               StartDate = DateTime.Now.AddMonths(-2)
                                           },
                                       new Apprenticeship
                                           {
                                               Id = 007,
                                               FirstName = "WaitingToStart",
                                               PaymentStatus = PaymentStatus.Active,
                                               StartDate = DateTime.Now.AddMonths(2),
                                               DataLockCourseTriaged = true
                                           }
                                   };
            _sut = new ApprenticeshipFilterService(new FacetMapper());
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldNotFilterIfNoApprenticeshipStatusSelected(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery { ApprenticeshipStatuses = new List<ApprenticeshipStatus>() };

            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(2);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterApprenticeshipStatusSelected(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery
                            {
                                ApprenticeshipStatuses =
                                    new List<ApprenticeshipStatus>(
                                    new[] { ApprenticeshipStatus.Live })
                            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.Single().FirstName.Should().Be("Live");
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterApprenticeshipStatusSelectedAllButOneSelected(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery
                            {
                                ApprenticeshipStatuses =
                                    new List<ApprenticeshipStatus>(
                                    new[]
                                        {
                                            ApprenticeshipStatus.Live,
                                            ApprenticeshipStatus.Finished,
                                            ApprenticeshipStatus.Paused,
                                            ApprenticeshipStatus.Stopped
                                        })
                            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.Single().FirstName.Should().Be("Live");
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterRecordStatusOnChangeRequested(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery
            {
                RecordStatuses = new List<RecordStatus>(new [] { RecordStatus.ChangeRequested } )
            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterRecordStatusOnNothing(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery
            {
                RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.NoActionNeeded })
            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterRecordStatusOnIlrDataMismatch(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship { FirstName = "ILR Data Mismatch", DataLockPrice = true});
            var query = new ApprenticeshipSearchQuery
            {
                RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.IlrDataMismatch })
            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.FirstOrDefault().FirstName.Should().Be("ILR Data Mismatch");
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterRecordStatusOnMisMatch(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship { FirstName = "ILR Data Mismatch", DataLockCourse = true});

            var query = new ApprenticeshipSearchQuery
            {
                RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.ChangeRequested, RecordStatus.IlrDataMismatch,  })
            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(2);
            result.PageOfResults.Count(m => m.FirstName == "ILR Data Mismatch").Should().Be(1);
            result.PageOfResults.Count(m => m.FirstName == "WaitingToStart").Should().Be(1);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterOnlyOneInstanceOfApprenticeship(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship { FirstName = "ILR Data Mismatch", DataLockCourseTriaged = true, PendingUpdateOriginator = caller});

            var query = new ApprenticeshipSearchQuery
            {
                RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.ChangeRequested, RecordStatus.IlrDataMismatch, })
            };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(2);
            result.PageOfResults.Count(m => m.FirstName == "ILR Data Mismatch").Should().Be(1);
            result.PageOfResults.Count(m => m.FirstName == "WaitingToStart").Should().Be(1);
        }


        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFindNothingWhenFilterOnChangesPending(Originator caller)
        {
            var query = new ApprenticeshipSearchQuery { RecordStatuses = new List<RecordStatus>(new [] { RecordStatus.ChangesPending, } ) };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(0);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFindChangesPending(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship
                                     {
                                        Id = 009,
                                        FirstName = "ChangesPending",
                                        PendingUpdateOriginator = caller

                                     });
            var query = new ApprenticeshipSearchQuery { RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.ChangesPending, }) };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.Single().FirstName.Should().Be("ChangesPending");
        }

        [TestCase(Originator.Provider, Originator.Employer)]
        [TestCase(Originator.Employer, Originator.Provider)]
        public void ShouldFindChangesForReview(Originator caller, Originator otherPart)
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 009,
                FirstName = "ChangesForReview",
                PendingUpdateOriginator = otherPart

            });
            var query = new ApprenticeshipSearchQuery { RecordStatuses = new List<RecordStatus>(new[] { RecordStatus.ChangesForReview, }) };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.Single().FirstName.Should().Be("ChangesForReview");
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterOnTrainingCode(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 009,
                FirstName = "Should find",
                TrainingCode = "123-00-009"

            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10"

            });

            var query = new ApprenticeshipSearchQuery { TrainingCourses = new List<string>(new [] { "123-00-009", "35", "2" } )};
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.Single().Id.Should().Be(009);
        }

        [TestCase(Originator.Provider)]
        [TestCase(Originator.Employer)]
        public void ShouldFilterOnTrainingCodeButFindNothing(Originator caller)
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 082,
                FirstName = "Should find",
                TrainingCode = "123-00-082"

            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10"

            });

            var query = new ApprenticeshipSearchQuery { TrainingCourses = new List<string>(new[] { "123-00-009", "35", "2" }) };
            var result = _sut.Filter(_apprenticeships, query, caller);

            result.PageOfResults.Count.Should().Be(0);
        }


        [Test]
        public void ShouldFilterOnEmployerIdWhenProvider()
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 082,
                FirstName = "Should find",
                TrainingCode = "123-00-082",
                EmployerAccountId = 999L,
                LegalEntityName = "Employer 999",
                LegalEntityId = "09990"
            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10",
                EmployerAccountId = 001L,
                LegalEntityName = "Employer 001",
                LegalEntityId = "Abba-"

            });

            var query = new ApprenticeshipSearchQuery { EmployerOrganisationIds = new List<string> { "09990" } };
            var result = _sut.Filter(_apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.FirstOrDefault().LegalEntityName.Should().Be("Employer 999");
        }

        [Test]
        public void ShouldNotFilterOnEmployerIdWhenCallerIsEmployer()
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 082,
                FirstName = "Should find",
                TrainingCode = "123-00-082",
                EmployerAccountId = 999L,
                LegalEntityName = "Employer 999"
            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10",
                EmployerAccountId = 001L,
                LegalEntityName = "Employer 001"

            });

            var query = new ApprenticeshipSearchQuery { EmployerOrganisationIds = new List<string> { "999" } };
            var result = _sut.Filter(_apprenticeships, query, Originator.Employer);

            result.PageOfResults.Count.Should().Be(_apprenticeships.Count);
        }

        [Test]
        public void ShouldFilterOnProviderIdWhenEmployer()
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 082,
                FirstName = "Should find",
                TrainingCode = "123-00-082",
                EmployerAccountId = 999L,
                LegalEntityName = "Employer 999",
                ProviderId = 008,
                ProviderName = "Provider 008"
            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10",
                EmployerAccountId = 001L,
                LegalEntityName = "Employer 001",
                ProviderId = 007,
                ProviderName = "Provider 007"

            });

            var query = new ApprenticeshipSearchQuery { TrainingProviderIds = new List<long> { 007 } };
            var result = _sut.Filter(_apprenticeships, query, Originator.Employer);

            result.PageOfResults.Count.Should().Be(1);
            result.PageOfResults.FirstOrDefault().ProviderName.Should().Be("Provider 007");
        }

        [Test]
        public void ShouldNotFilterOnProviderIdWhenCallerIsProvider()
        {
            _apprenticeships.Add(new Apprenticeship
            {
                Id = 082,
                FirstName = "Should find",
                TrainingCode = "123-00-082",
                EmployerAccountId = 999L,
                LegalEntityName = "Employer 999",
                ProviderId = 008,
                ProviderName = "Provider 008"
            });

            _apprenticeships.Add(new Apprenticeship
            {
                Id = 010,
                FirstName = "Should not find",
                TrainingCode = "10",
                EmployerAccountId = 001L,
                LegalEntityName = "Employer 001",
                ProviderId = 007,
                ProviderName = "Provider 007"

            });

            var query = new ApprenticeshipSearchQuery { TrainingProviderIds = new List<long> { 007 } };
            var result = _sut.Filter(_apprenticeships, query, Originator.Provider);

            result.PageOfResults.Count.Should().Be(_apprenticeships.Count);
        }
    }
}