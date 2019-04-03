using System;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Commitments.Application.Interfaces;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Domain.Interfaces;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Commitments.Application.Services
{
    public class V2EventsPublisher : IV2EventsPublisher
    {
        private readonly IEndpointInstance _endpointInstance;
        private readonly ICommitmentsLogger _logger;


        public V2EventsPublisher(IEndpointInstance endpointInstance, ICommitmentsLogger logger)
        {
            _endpointInstance = endpointInstance;
            _logger = logger;
        }

        public async Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship)
        {
            var logMessage = $"Publish IApprenticeshipDeletedEvent message. Provider:{apprenticeship.ProviderId} Commitment:{commitment.Id} Apprenticeship:{apprenticeship.Id}";

            try
            {
                await _endpointInstance.Publish<IApprenticeshipDeletedEvent>(ev =>
                {
                    ev.CourseCode = apprenticeship.TrainingCode;
                    ev.Apprenticeship = apprenticeship.Id;
                    ev.CommitmentId = commitment.Id;
                    ev.CourseStartDate = apprenticeship.StartDate;
                    ev.DeletedOn = DateTime.UtcNow;
                    ev.ReservationId = apprenticeship.ReservationId;
                    ev.Uln = apprenticeship.ULN;
                });

                _logger.Info($"{logMessage} successful");
            }
            catch(Exception e)
            {
                _logger.Error(e, $"{logMessage} failed");
            }
        }
    }
}