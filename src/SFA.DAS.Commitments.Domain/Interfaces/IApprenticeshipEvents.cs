using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Commitments.Domain.Entities;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IApprenticeshipEvents
    {
        Task PublishEvent(Commitment commitment, Apprenticeship apprenticeship, string @event);
        Task BulkPublishEvent(Commitment commitment, IList<Apprenticeship> apprenticeships, string @event);

        Task PublishDeletionEvent(Commitment commitment, Apprenticeship apprenticeship, string @event);
        Task BulkPublishDeletionEvent(Commitment commitment, IList<Apprenticeship> apprenticeships, string @event);
        Task PublishChangeApprenticeshipStatusEvent(Commitment commitment, Apprenticeship apprenticeship, PaymentStatus paymentStatus);
    }
}
