using System.Threading;
using System.Threading.Tasks;
using SFA.DAS.CommitmentsV2.Domain.Entities.EditApprenticeshipValidation;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Domain.Interfaces
{
    public interface IEditApprenticeshipValidationService
    {
        Task<EditApprenticeshipValidationResult> Validate(EditApprenticeshipValidationRequest request, CancellationToken cancellationToken, Party party = Party.None);
    }
}
