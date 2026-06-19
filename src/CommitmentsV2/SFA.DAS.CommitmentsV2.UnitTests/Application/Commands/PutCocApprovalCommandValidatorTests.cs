using System.Linq.Expressions;
using FluentValidation.TestHelper;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Application.Commands.CocApprovals;
using SFA.DAS.CommitmentsV2.Application.Commands.CreateChangeOfPartyRequest;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.UnitTests.Application.Commands;

[TestFixture]
[Parallelizable]
public class PutCocApprovalCommandValidatorTests
{
    [Test]
    public void Validate_ApprenticeshipPasses_WhenNotNull()
    {
        var command = new PutCocApprovalCommand
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
        var command = new PutCocApprovalCommand();

        AssertValidationResult(r => r.CocApprovalDetails, command, false);
    }

    [Test]
    public void Validate_ApprenticeshipFails_WhenNull()
    {
        var command = new PutCocApprovalCommand
        {
            CocApprovalDetails = new CocApprovalDetails
            {
                Apprenticeship = null
            }
        };

        AssertValidationResult(r => r.CocApprovalDetails.Apprenticeship, command, false);
    }

    [TestCase(1234, false)]
    public void Validate_ApprenticeshipId_ShouldBeValidated(long providerId, bool isValid)
    {
        var command = new PutCocApprovalCommand
        {
            CocApprovalDetails = new CocApprovalDetails
            {
                Apprenticeship = new Apprenticeship
                {
                    Cohort = new Cohort
                    {
                        ProviderId = providerId
                    }
                }
            }
        };

        AssertValidationResult(r => r.CocApprovalDetails.Apprenticeship.Cohort.ProviderId, command, isValid);
    }

    [Test]
    public void Validate_DataWithNoEffectiveFromDate_ShouldBeValid()
    {
        var command = new PutCocApprovalCommand
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
                        ChangeType = "TNP1",
                        Data = new CocData
                        {
                            Old = "10",
                            New = "20",
                            EffectiveFromDate = null
                        }
                    }
                }
            }
        };

        AssertValidationPropertyHasMessage("CocApprovalDetails.ApprovalFieldChanges[0]", command, true);
    }

    [Test]
    public void Validate_DataWithEffectiveFromDateBefore_ShouldBeInvalid()
    {
        var command = new PutCocApprovalCommand
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
                    Uln = "1234567890"
                },
                ApprovalFieldChanges = new List<CocApprovalFieldChange>
                {
                    new CocApprovalFieldChange
                    {
                        ChangeType = "TNP1",
                        Data = new CocData
                        {
                            Old = "10",
                            New = "20",
                            EffectiveFromDate = DateTime.Today.AddDays(-1)
                        }
                    }
                }
            }
        };

        AssertValidationPropertyHasMessage("CocApprovalDetails.ApprovalFieldChanges[0]", command, false);
    }

    [Test]
    public void Validate_DataWithEffectiveFromDateAfterStart_ShouldBeValid()
    {
        var command = new PutCocApprovalCommand
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
                    EndDate = DateTime.Today.AddMonths(10),
                    Uln = "1234567890"
                },
                ApprovalFieldChanges = new List<CocApprovalFieldChange>
                {
                    new CocApprovalFieldChange
                    {
                        ChangeType = "TNP1",
                        Data = new CocData
                        {
                            Old = "10",
                            New = "20",
                            EffectiveFromDate = DateTime.Today.AddDays(1)
                        }
                    }
                }
            }
        };

        AssertValidationPropertyHasMessage("CocApprovalDetails.ApprovalFieldChanges[0]", command, true);
    }

    [TestCase("2026-04-01", null, null, "2026-09-01", true)]
    [TestCase("2026-04-01", null, null, "2026-03-01", false)]
    [TestCase("2026-04-01", null, "2026-05-01", "2026-03-01", true)]
    [TestCase("2026-04-01", null, "2026-03-29", "2026-09-01", false)]
    [TestCase("2026-04-01", "2026-06-01", "2026-03-01", "2026-03-01", true)]
    [TestCase("2026-04-01", "2026-06-01", null, "2026-03-01", true)]
    [TestCase("2026-04-01", "2026-03-28", "2026-06-01", "2026-06-01", false)]
    [TestCase("2026-04-01", "2026-03-28", null, "2026-06-01", false)]
    public void Validate_DataWithEffectiveFromDateBeforeBestEndDate_ShouldBeHaveExpectedValidState(DateTime? effectiveFromDate, DateTime? completionDate, DateTime? stopDate, DateTime endDate, bool isValid)
    {
        var command = new PutCocApprovalCommand
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
                    StartDate = endDate.AddYears(-1),
                    EndDate = endDate,
                    StopDate = stopDate,
                    CompletionDate = completionDate,
                    Uln = "1234567890"
                },
                ApprovalFieldChanges = new List<CocApprovalFieldChange>
                {
                    new CocApprovalFieldChange
                    {
                        ChangeType = "TNP1",
                        Data = new CocData
                        {
                            Old = "10",
                            New = "20",
                            EffectiveFromDate = effectiveFromDate
                        }
                    }
                }
            }
        };

        AssertValidationPropertyHasMessage("CocApprovalDetails.ApprovalFieldChanges[0]", command, isValid);
    }




    private void AssertValidationResult<T>(Expression<Func<PutCocApprovalCommand, T>> property,
        PutCocApprovalCommand command, bool isValid)
    {
        var validator = new PutCocApprovalCommandValidator();
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

    private void AssertValidationPropertyHasMessage(string property, PutCocApprovalCommand command, bool isValid)
    {
        var validator = new PutCocApprovalCommandValidator();
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