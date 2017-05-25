using FluentValidation;

namespace SFA.DAS.Commitments.Application.Commands.CreateBulkUpload
{
    public class CreateBulkUploadValidator : AbstractValidator<CreateBulkUploadCommand>
    {
        public CreateBulkUploadValidator()
        {
            RuleFor(x => x.CommitmentId).Must(m => m > 0);
            RuleFor(x => x.ProviderId).Must(m => m > 0);
            RuleFor(x => x.BulkUploadFile).NotNull();
        }
    }
}