using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NServiceBus;
using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.CommitmentsV2.Messages.Events;

namespace SFA.DAS.Commitments.Application.Interfaces
{
    /// <summary>
    ///     This represents a service that can publish the V2 events via nservicebus.
    /// </summary>
    public interface IV2EventsPublisher
    {
        Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship);
    }

    public class V2EventsPublisher : IV2EventsPublisher
    {
        private readonly IEndpointInstance _endpointInstance;
       
        public V2EventsPublisher(IEndpointInstance endpointInstance)
        {
            this._endpointInstance = endpointInstance;
        }

        public async Task PublishApprenticeshipDeleted(Commitment commitment, Apprenticeship apprenticeship)
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
        }
    }
}
