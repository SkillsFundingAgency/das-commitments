using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Validators;
using SFA.DAS.CommitmentsV2.Domain.ValueObjects;
using SFA.DAS.CommitmentsV2.Models;
using SFA.DAS.Reservations.Api.Client;
using SFA.DAS.Reservations.Api.Client.Types;

namespace SFA.DAS.CommitmentsV2.Validators
{
    public class AddDraftApprenticeshipModelValidator : AbstractValidator<AddDraftApprenticeshipModel>
    {
        public AddDraftApprenticeshipModelValidator(
            IValidator<DraftApprenticeshipDetails> draftApprenticeshipDetailsValidator,
            IReservationsApiClient reservationsApiClient)
        {
            RuleFor(ctx => ctx.DraftApprenticeshipDetails).NotNull();
            RuleFor(ctx => ctx.Commitment).NotNull();

            RuleFor(ctx => ctx.DraftApprenticeshipDetails)
                .SetValidator(draftApprenticeshipDetailsValidator)
                .OverridePropertyName("")
                .When(ctx => ctx.DraftApprenticeshipDetails != null);

            RuleFor(ctx => ctx.DraftApprenticeshipDetails.ReservationId)
                .Must((ctx, reservationId) => ctx.Commitment.Apprenticeship.All(apprenticeship => apprenticeship.ReservationId != reservationId))
                .When(ctx => ctx.DraftApprenticeshipDetails != null)
                .WithMessage("ULN must be unique within the cohort");

            RuleFor(ctx => ctx)
                .CustomAsync(async (ctx, customContext, cancellationToken) => await ValidateReservationId(reservationsApiClient, ctx, customContext, cancellationToken));
        }

        private static async Task<bool> ValidateReservationId(
            IReservationsApiClient reservationsApiClient,
            AddDraftApprenticeshipModel ctx, 
            CustomContext customContext, 
            CancellationToken cancellationToken)
        {
            if (ctx.DraftApprenticeshipDetails.ReservationId == null || ctx.DraftApprenticeshipDetails.ReservationId == Guid.Empty)
            {
                return true;
            }

            var reservationValidationMessage = new ValidationReservationMessage
            {
                AccountId = ctx.Commitment.EmployerAccountId,
                StartDate = ctx.DraftApprenticeshipDetails.StartDate,
                ProviderId = ctx.Commitment.ProviderId ?? -1,
                CourseCode = ctx.DraftApprenticeshipDetails.TrainingProgramme?.CourseCode,
                ReservationId = ctx.DraftApprenticeshipDetails.ReservationId.Value,
                AccountLegalEntityPublicHashedId = ctx.Commitment.AccountLegalEntityPublicHashedId
            };

            var validationResult =
                await reservationsApiClient.ValidateReservation(reservationValidationMessage,
                    cancellationToken);

            if (validationResult.HasErrors)
            {
                foreach (var error in validationResult.ValidationErrors)
                {
                    customContext.AddFailure(error.PropertyName, error.Reason);
                }
            }

            return validationResult.IsOkay;
        }
    }
}