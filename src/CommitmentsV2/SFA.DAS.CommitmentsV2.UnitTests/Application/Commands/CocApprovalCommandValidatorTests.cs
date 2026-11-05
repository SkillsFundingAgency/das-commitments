using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Models;
using System.Linq.Expressions;
using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands
{
    [TestFixture]
    [Parallelizable]
    public class CocApprovalCommandValidatorTests
    {
        [Test]
        public void Validate_CocApprovalCommandPasses_WhenActionCancelPrevious()
        {
            var command = new CocApprovalCommand
            {
                Action = AggregationAction.CancelPrevious,
                CocApprovalDetails = null
            };

            AssertValidationResult(r => r.CocApprovalDetails, command, true);
        }

        [Test]
        public void Validate_CocApprovalCommandFails_WhenCocApprovalDetailsNull()
        {
            var command = new CocApprovalCommand
            {
                Action = AggregationAction.CreateNew,
                CocApprovalDetails = null
            };

            AssertValidationResult(r => r.CocApprovalDetails, command, false);
        }

        [Test]
        public void Validate_CocApprovalCommandPasses_WhenNoCocApprovalDetails()
        {
            var command = new CocApprovalCommand
            {
                Action = AggregationAction.CreateNew,
                CocApprovalDetails = new CocApprovalDetails()
            };

            AssertValidationResult(r => r.CocApprovalDetails, command, true);
        }

        [Test]
        public void Validate_ApprenticeshipPasses_WhenNotNull()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        }
                    }
                }
            };

            AssertValidationResult(r => r.CocApprovalDetails.Apprenticeship, command, true);
        }

        [Test]
        public void Validate_CocApprovalCommandFails_WhenNull()
        {
            var command = new CocApprovalCommand();

            AssertValidationResult(r => r.CocApprovalDetails, command, false);
        }

        [Test]
        public void Validate_ApprenticeshipFails_WhenNull()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    Apprenticeship = null
                }
            };

            AssertValidationResult(r => r.CocApprovalDetails.Apprenticeship, command, false);
        }

        [Test]
        public void Validate_ULN_ShouldBeValid()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    ProviderId = 12345,
                    ULN = "1234567890",
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        },
                        StartDate = DateTime.Today,
                        Uln = "1234567890",
                        CourseCode = "LarsCode"
                    },
                    Course = new Course
                    {
                        LarsCode = "LarsCode",
                        LearningType = LearningType.Apprenticeship
                    },
                    ApprovalFieldChanges = new List<CocApprovalFieldChange>
                    {
                        new CocApprovalFieldChange
                        {
                            ChangeType = "Firstname",
                            Data = new CocData
                            {
                                Old = "Ann",
                                New = "Anne",
                            }
                        }
                    }
                }
            };

            AssertValidationPropertyHasMessage("CocApprovalDetails.ULN", command, true);
        }

        [Test]
        public void Validate_ULN_ShouldBeInvalid()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    ProviderId = 12345,
                    ULN = "1234567890",
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        },
                        StartDate = DateTime.Today,
                        Uln = "123456789",
                        CourseCode = "LarsCode"
                    },
                    Course = new Course
                    {
                        LarsCode = "LarsCode",
                        LearningType = LearningType.Apprenticeship
                    },
                    ApprovalFieldChanges = new List<CocApprovalFieldChange>
                    {
                        new CocApprovalFieldChange
                        {
                            ChangeType = "Firstname",
                            Data = new CocData
                            {
                                Old = "Ann",
                                New = "Anne",
                            }
                        }
                    }
                }
            };

            AssertValidationPropertyHasMessage("CocApprovalDetails.ULN", command, false);
        }

        [Test]
        public void Validate_LearningType_ShouldBeValid()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    ProviderId = 12345,
                    ULN = "1234567890",
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        },
                        StartDate = DateTime.Today,
                        Uln = "1234567890",
                        CourseCode = "LarsCode"
                    },
                    Course = new Course
                    {
                        LarsCode = "LarsCode",
                        LearningType = LearningType.Apprenticeship
                    },
                    ApprovalFieldChanges = new List<CocApprovalFieldChange>
                    {
                        new CocApprovalFieldChange
                        {
                            ChangeType = "Firstname",
                            Data = new CocData
                            {
                                Old = "Ann",
                                New = "Anne",
                            }
                        }
                    }
                }
            };

            AssertValidationPropertyHasMessage("CocApprovalDetails.LearningType", command, true);
        }

        [Test]
        public void Validate_LearningType_ShouldBeInvalid()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    ProviderId = 12345,
                    ULN = "1234567890",
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        },
                        StartDate = DateTime.Today,
                        Uln = "123456789",
                        CourseCode = "LarsCode"
                    },
                    Course = new Course
                    {
                        LarsCode = "LarsCode",
                        LearningType = LearningType.FoundationApprenticeship
                    },
                    ApprovalFieldChanges = new List<CocApprovalFieldChange>
                    {
                        new CocApprovalFieldChange
                        {
                            ChangeType = "Firstname",
                            Data = new CocData
                            {
                                Old = "Ann",
                                New = "Anne",
                            }
                        }
                    }
                }
            };

            AssertValidationPropertyHasMessage("CocApprovalDetails.LearningType", command, false);
        }

        [Test]
        public void Validate_ApprenticeshipId_ShouldBeValidated()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 1234
                        }
                    }
                }
            };

            AssertValidationResult(r => r.CocApprovalDetails.ProviderId, command, false);
        }

        [Test]
        public void Validate_DataWithNoEffectiveFromDate_ShouldBeValid()
        {
            var command = new CocApprovalCommand
            {
                CocApprovalDetails = new CocApprovalDetails
                {
                    Apprenticeship = new Apprenticeship
                    {
                        Cohort = new Cohort
                        {
                            ProviderId = 12345
                        }
                    },
                    ApprovalFieldChanges = new List<CocApprovalFieldChange>
                {
                    new CocApprovalFieldChange
                    {
                        ChangeType = "Firstname",
                        Data = new CocData
                        {
                            Old = "Ann",
                            New = "Anne",
                        }
                    }
                }
                }
            };

            AssertValidationPropertyHasMessage("CocApprovalDetails.ApprovalFieldChanges[0]", command, true);
        }

        private void AssertValidationResult<T>(Expression<Func<CocApprovalCommand, T>> property,
            CocApprovalCommand command, bool isValid)
        {
            var validator = new CocApprovalCommandValidator();
            var result = validator.TestValidate(command);

            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(property);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(property);
            }
        }

        private void AssertValidationPropertyHasMessage(string property, CocApprovalCommand command, bool isValid)
        {
            var validator = new CocApprovalCommandValidator();
            var result = validator.TestValidate(command);

            if (isValid)
            {
                result.ShouldNotHaveValidationErrorFor(property);
            }
            else
            {
                result.ShouldHaveValidationErrorFor(property);
            }
        }
    }
}
