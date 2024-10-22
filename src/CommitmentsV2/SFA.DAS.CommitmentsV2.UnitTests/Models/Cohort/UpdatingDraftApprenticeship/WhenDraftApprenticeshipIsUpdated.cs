using MoreLinq;
using SFA.DAS.CommitmentsV2.Domain.Entities;
using SFA.DAS.CommitmentsV2.Domain.Extensions;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.CommitmentsV2.Types;
using SFA.DAS.UnitOfWork.Context;

namespace SFA.DAS.CommitmentsV2.UnitTests.Models.Cohort.UpdatingDraftApprenticeship
{
    public class WhenDraftApprenticeshipIsUpdated
    {
        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        //In this test the Cost field is used to represent any field for which updates require the approval
        //of the other party (resulting in the DraftApprenticeship object's AgreementStatus property to be reset)
        //Further tests can be found in: Domain\DraftApprenticeship\WhenDraftApprenticeshipIsUpdated.cs
        public void UpdateDraftApprenticeship_Cost_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyCohortIsUnapproved();
        }

        [Test]
        public void UpdateDraftApprenticeship_Provider_Changes_TrainingPrice_Resets_OtherParty_Approval()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipTrainingPrice();

            fixture.VerifyCohortIsUnapproved();
        }

        [Test]
        public void UpdateDraftApprenticeship_Provider_Changes_EndPointAssessmentPrice_Resets_OtherParty_Approval()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipEndPointAssessmentPrice();

            fixture.VerifyCohortIsUnapproved();
        }

        [Test]
        public void UpdateDraftApprenticeship_Employer_No_Cost_Change_Does_Not_Reset_OtherParty_Approval()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Employer);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipEmployerMakesNoChangeToCostAndDoesNotKnowAboutPriceBreakdown();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [Test]
        public void UpdateDraftApprenticeship_Employer_Cost_Change_Sets_Flag()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Employer);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .WithFlexiPaymentPilotFlagSetToTrue()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyEmployerHasEditedCostFlag(true);
        }

        [Test]
        public void UpdateDraftApprenticeship_Provider_Cost_Change_Does_Not_Set_Employer_Flag()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .WithFlexiPaymentPilotFlagSetToTrue()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyEmployerHasEditedCostFlag(false);
        }

        [Test]
        public void UpdateDraftApprenticeship_Setting_TrainingPrice_And_EPAPrice_Resets_EmployerHasEditedCost_Flag()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .WithFlexiPaymentPilotFlagSetToTrue()
                .WithEmployerHasEditedCostFlagSetToTrue()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyEmployerHasEditedCostFlag(false);
        }

        [Test]
        public void UpdateDraftApprenticeship_Employer_Cost_Change_Blanks_TrainingPrice_And_EPAPrice_For_Flexi_Payments_Pilot_Apprenticeship()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Employer);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .WithFlexiPaymentPilotFlagSetToTrue()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyTrainingPriceAndEPAPriceAreNull();
        }

        [Test]
        public void UpdateDraftApprenticeship_Provider_Cost_Change_Does_Not_Blank_TrainingPrice_And_EPAPrice_For_Flexi_Payments_Pilot_Apprenticeship()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .WithFlexiPaymentPilotFlagSetToTrue()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyTrainingPriceAndEPAPriceAreNotNull();
        }

        [Test]
        public void UpdateDraftApprenticeship_Employer_Cost_Change_Does_Not_Blank_TrainingPrice_And_EPAPrice_For_Non_Pilot_Apprenticeship()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Employer);

            fixture
                .WithSingleExistingDraftApprenticeship()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyTrainingPriceAndEPAPriceAreNotNull();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_CombinedTotalPrice_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipTrainingAndEndPointAssessmentPriceButTotalPriceUnchanged();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_Reference_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipReference();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [Test]
        public void UpdateDraftApprenticeship_Uln_By_Provider_Does_Not_Reset_Employer_Approval()
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(Party.Provider);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipUln();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentPrice_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipEmploymentPrice();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentEndDate_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipEmploymentEndDate();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_PilotStatusChange_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithActualStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotStatusAndCopyStartDate();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentStartDate_Day_Change_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipStartDateDay();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentStartDate_Month_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipStartDateMonth();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentStartDate_Year_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipStartDateYear();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentActualStartDate_Day_Change_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithActualStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateDay();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentActualStartDate_Month_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithActualStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateMonth();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentActualStartDate_Year_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithActualStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateYear();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EndDate_Day_Change_Does_Not_Reset_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithEndDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipEndDateDay();

            fixture.VerifyCohortIsApprovedByOtherParty();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EndDate_Month_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithEndDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipEndDateMonth();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EndDate_Year_Change_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithEndDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateFlexiPaymentPilotDraftApprenticeshipEndDateYear();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_EmploymentStartDate_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithStartDate()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipStartDate();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_DeliveryModel_Resets_OtherParty_Approval(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithPriorApprovalOfOtherParty()
                .UpdateDraftApprenticeshipDeliveryModel();

            fixture.VerifyCohortIsUnapproved();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_Tracks_State_Changes(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .UpdateDraftApprenticeshipCost();

            fixture.VerifyDraftApprenticeshipTracking();
            fixture.VerifyCohortTracking();
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_With_ChangeOfParty_FirstName_Throws_No_Errors(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithChangeOfPartyCohort()
                .UpdateDraftApprenticeshipFirstName();

            Assert.That(fixture.Exception, Is.Null);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_With_ChangeOfParty_LastName_Throws_No_Errors(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithChangeOfPartyCohort()
                .UpdateDraftApprenticeshipLastName();

            Assert.That(fixture.Exception, Is.Null);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_With_ChangeOfParty_DateOfBirth_Throws_No_Errors(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithChangeOfPartyCohort()
                .UpdateDraftApprenticeshipDateOfBirth();

            Assert.That(fixture.Exception, Is.Null);
        }

        [TestCase(Party.Employer)]
        [TestCase(Party.Provider)]
        public void UpdateDraftApprenticeship_With_ChangeOfParty_CourseCode_Throws(Party modifyingParty)
        {
            var fixture = new UpdatingDraftApprenticeshipTestFixture(modifyingParty);

            fixture
                .WithExistingDraftApprenticeships()
                .WithChangeOfPartyCohort()
                .UpdateDraftApprenticeshipCourseCode();

            Assert.That(fixture.Exception, Is.Not.Null);
        }

        private class UpdatingDraftApprenticeshipTestFixture
        {
            private readonly Fixture _autoFixture = new Fixture();
            
            // If you use DateTime.Now, some of the tests will fail if run on the last day of the month.
            private readonly DateTime _referenceDate = new DateTime(2024,10,10);
            private UnitOfWorkContext UnitOfWorkContext { get; }
            private UserInfo UserInfo { get; }
            private Party ModifyingParty { get; }
            private CommitmentsV2.Models.Cohort Cohort { get; }
            public Exception Exception { get; private set; }

            public UpdatingDraftApprenticeshipTestFixture(Party modifyingParty)
            {
                UnitOfWorkContext = new UnitOfWorkContext();

                ModifyingParty = modifyingParty;
                Cohort = new CommitmentsV2.Models.Cohort
                {
                    WithParty = modifyingParty,
                    ProviderId = 1
                };

                UserInfo = new UserInfo
                {
                    UserId = "user-1",
                    UserDisplayName = "Tester",
                    UserEmail = "tester@test.com"
                };
            }

            public UpdatingDraftApprenticeshipTestFixture WithExistingDraftApprenticeships()
            {
                Cohort.Apprenticeships.Clear();
                for (var i = 0; i < 10; i++)
                {
                    var trainingPrice = _autoFixture.Create<int>();
                    var epaPrice = _autoFixture.Create<int>();

                    var apprenticeship = new DraftApprenticeship
                    {
                        Id = i,
                        FirstName = _autoFixture.Create<string>(),
                        LastName = _autoFixture.Create<string>(),
                        Cost = trainingPrice + epaPrice,
                        CourseCode = _autoFixture.Create<string>(),
                        CourseName = _autoFixture.Create<string>(),
                        DeliveryModel = DeliveryModel.Regular,
                        FlexibleEmployment = new FlexibleEmployment
                        {
                            EmploymentEndDate = _autoFixture.Create<DateTime>(),
                            EmploymentPrice = _autoFixture.Create<int>()
                        },
                        DateOfBirth = _referenceDate.AddYears(-17),
                        IsOnFlexiPaymentPilot = false,
                        TrainingPrice = trainingPrice,
                        EndPointAssessmentPrice = epaPrice  
                    };
                    
                    Cohort.Apprenticeships.Add(apprenticeship);
                }
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithSingleExistingDraftApprenticeship()
            {
                Cohort.Apprenticeships.Clear();

                var trainingPrice = _autoFixture.Create<int>();
                var epaPrice = _autoFixture.Create<int>();

                Cohort.Apprenticeships.Add(new DraftApprenticeship
                {
                    Id = 1,
                    FirstName = _autoFixture.Create<string>(),
                    LastName = _autoFixture.Create<string>(),
                    Cost = trainingPrice + epaPrice,
                    CourseCode = _autoFixture.Create<string>(),
                    CourseName = _autoFixture.Create<string>(),
                    DeliveryModel = DeliveryModel.Regular,
                    FlexibleEmployment = new FlexibleEmployment
                    {
                        EmploymentEndDate = _autoFixture.Create<DateTime>(),
                        EmploymentPrice = _autoFixture.Create<int>()
                    },
                    DateOfBirth = DateTime.Now.AddYears(-17),
                    IsOnFlexiPaymentPilot = false,
                    TrainingPrice = trainingPrice,
                    EndPointAssessmentPrice = epaPrice
                });

                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithStartDate()
            {
                var nextMonth = _referenceDate.AddMonths(1);
                Cohort.Apprenticeships.ForEach(c => c.StartDate = new DateTime(nextMonth.Year, nextMonth.Month, 1));
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithActualStartDate()
            {
                var nextMonth = _referenceDate.AddMonths(1);
                Cohort.Apprenticeships.ForEach(c => c.ActualStartDate = new DateTime(nextMonth.Year, nextMonth.Month, 15));
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithEndDate()
            {
                var nextYear = _referenceDate.AddYears(1);
                Cohort.Apprenticeships.ForEach(c => c.EndDate = nextYear);
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithChangeOfPartyCohort()
            {
                Cohort.ChangeOfPartyRequestId = _autoFixture.Create<long>();
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithPriorApprovalOfOtherParty()
            {
                Cohort.Approvals = ModifyingParty.GetOtherParty();
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithFlexiPaymentPilotFlagSetToTrue()
            {
                Cohort.Apprenticeships.ForEach(x => x.IsOnFlexiPaymentPilot = true);
                return this;
            }

            public UpdatingDraftApprenticeshipTestFixture WithEmployerHasEditedCostFlagSetToTrue()
            {
                Cohort.Apprenticeships.ForEach(x => x.EmployerHasEditedCost = true);
                return this;
            }

            public void UpdateDraftApprenticeshipCost()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Cost = details.Cost + 1 ?? 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipTrainingPrice()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.IsOnFlexiPaymentPilot = true;
                details.TrainingPrice = details.TrainingPrice + 1 ?? 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipEndPointAssessmentPrice()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.IsOnFlexiPaymentPilot = true;
                details.EndPointAssessmentPrice = details.EndPointAssessmentPrice + 1 ?? 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipTrainingAndEndPointAssessmentPriceButTotalPriceUnchanged()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.IsOnFlexiPaymentPilot = true;
                details.TrainingPrice += 1;
                details.EndPointAssessmentPrice -= 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipEmployerMakesNoChangeToCostAndDoesNotKnowAboutPriceBreakdown()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.IsOnFlexiPaymentPilot = true;
                details.EndPointAssessmentPrice = null;
                details.TrainingPrice = null;
                Cohort.UpdateDraftApprenticeship(details, Party.Employer, UserInfo);
            }

            public void UpdateDraftApprenticeshipEmploymentPrice()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.EmploymentPrice = details.EmploymentPrice + 1 ?? 1;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipEmploymentEndDate()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.EmploymentEndDate = details.EmploymentEndDate.Value.AddDays(1);
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipStartDate()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.StartDate = details.StartDate.Value.AddMonths(1);
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipStartDateDay()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.StartDate.Value.AddDays(14);
                details.StartDate = null;
                details.IsOnFlexiPaymentPilot = true;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotStatusAndCopyStartDate()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.IsOnFlexiPaymentPilot = !details.IsOnFlexiPaymentPilot;
                details.StartDate = details.ActualStartDate.Value;
                details.ActualStartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipStartDateMonth()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.StartDate.Value.AddMonths(1);
                details.StartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }
            
            public void UpdateFlexiPaymentPilotDraftApprenticeshipStartDateYear()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.StartDate.Value.AddYears(1);
                details.StartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateDay()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.ActualStartDate.Value.AddDays(1);
                details.StartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateMonth()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.ActualStartDate.Value.AddMonths(1);
                details.StartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipActualStartDateYear()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.ActualStartDate = details.ActualStartDate.Value.AddYears(1);
                details.StartDate = null;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipEndDateDay()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.EndDate = details.EndDate.Value.AddDays(1);
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipEndDateMonth()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.EndDate = details.EndDate.Value.AddMonths(1);
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateFlexiPaymentPilotDraftApprenticeshipEndDateYear()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.EndDate = details.EndDate.Value.AddYears(1);
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipDeliveryModel()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.DeliveryModel = details.DeliveryModel != DeliveryModel.Regular ? DeliveryModel.Regular :
                    details.DeliveryModel != DeliveryModel.PortableFlexiJob ? DeliveryModel.FlexiJobAgency :
                    DeliveryModel.PortableFlexiJob;
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipFirstName()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.FirstName += "_modified";
                TryUpdate(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipLastName()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.LastName += "_modified";
                TryUpdate(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipDateOfBirth()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.DateOfBirth = details.DateOfBirth?.AddDays(1) ?? _autoFixture.Create<DateTime>();
                TryUpdate(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipCourseCode()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme("TEST", "TEST", ProgrammeType.Standard, DateTime.MinValue, DateTime.MaxValue);
                TryUpdate(details, ModifyingParty, UserInfo);
            }

            private void TryUpdate(DraftApprenticeshipDetails details, Party modifyingParty, UserInfo userInfo)
            {
                try
                {
                    Cohort.UpdateDraftApprenticeship(details, modifyingParty, userInfo);
                }
                catch (Exception ex)
                {
                    Exception = ex;
                }
            }

            public void UpdateDraftApprenticeshipReference()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Reference += " modified";
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            public void UpdateDraftApprenticeshipUln()
            {
                var details = GetRandomApprenticeshipDetailsFromCohort();
                details.Uln = "3688243446";
                Cohort.UpdateDraftApprenticeship(details, ModifyingParty, UserInfo);
            }

            private DraftApprenticeshipDetails GetRandomApprenticeshipDetailsFromCohort()
            {
                var random = new Random().Next(0, Cohort.Apprenticeships.Count);
                var target = Cohort.Apprenticeships.ToArray()[random];
                var details = ToApprenticeshipDetails(target as DraftApprenticeship);
                return details;
            }

            public void VerifyCohortIsUnapproved()
            {
                Assert.That(Cohort.Approvals, Is.EqualTo(Party.None));
            }

            public void VerifyCohortIsApprovedByOtherParty()
            {
                Assert.That(Cohort.Approvals.HasFlag(ModifyingParty.GetOtherParty()), Is.True);
            }

            public void VerifyDraftApprenticeshipTracking()
            {
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(DraftApprenticeship)), Is.Not.Null);
            }

            public void VerifyCohortTracking()
            {
                Assert.That(UnitOfWorkContext.GetEvents().SingleOrDefault(x => x is EntityStateChangedEvent @event
                                                                                    && @event.EntityType ==
                                                                                    nameof(Cohort)), Is.Not.Null);
            }

            public void VerifyEmployerHasEditedCostFlag(bool? flagValue)
            {
                Assert.That(Cohort.Apprenticeships.Single().EmployerHasEditedCost, Is.EqualTo(flagValue));
            }

            public void VerifyTrainingPriceAndEPAPriceAreNull()
            {
                Assert.Multiple(() =>
                {
                    Assert.That(Cohort.Apprenticeships.Single().TrainingPrice, Is.Null);
                    Assert.That(Cohort.Apprenticeships.Single().EndPointAssessmentPrice, Is.Null);
                });
            }

            public void VerifyTrainingPriceAndEPAPriceAreNotNull()
            {
                Assert.Multiple(() =>
                {
                    Assert.That(Cohort.Apprenticeships.Single().TrainingPrice, Is.Not.Null);
                    Assert.That(Cohort.Apprenticeships.Single().EndPointAssessmentPrice, Is.Not.Null);
                });
            }

            private static DraftApprenticeshipDetails ToApprenticeshipDetails(DraftApprenticeship draftApprenticeship)
            {
                return new DraftApprenticeshipDetails
                {
                    Id = draftApprenticeship.Id,
                    FirstName = draftApprenticeship.FirstName,
                    LastName = draftApprenticeship.LastName,
                    Uln = draftApprenticeship.Uln,
                    TrainingProgramme = new SFA.DAS.CommitmentsV2.Domain.Entities.TrainingProgramme(draftApprenticeship.CourseCode, "", ProgrammeType.Framework,
                        null, null),
                    DeliveryModel = draftApprenticeship.DeliveryModel,
                    EmploymentPrice = draftApprenticeship.FlexibleEmployment.EmploymentPrice,                    
                    EmploymentEndDate = draftApprenticeship.FlexibleEmployment.EmploymentEndDate,                    
                    Cost = (int?)draftApprenticeship.Cost,
                    StartDate = draftApprenticeship.StartDate,
                    ActualStartDate = draftApprenticeship.ActualStartDate,
                    EndDate = draftApprenticeship.EndDate,
                    DateOfBirth = draftApprenticeship.DateOfBirth,
                    Reference = draftApprenticeship.ProviderRef,
                    ReservationId = draftApprenticeship.ReservationId,
                    IsOnFlexiPaymentPilot = draftApprenticeship.IsOnFlexiPaymentPilot,
                    TrainingPrice = draftApprenticeship.TrainingPrice,
                    EndPointAssessmentPrice = draftApprenticeship.EndPointAssessmentPrice
                };
            }
        }
    }
}