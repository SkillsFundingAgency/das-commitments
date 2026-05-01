using System.Linq.Expressions;
using FluentValidation.TestHelper;
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
}