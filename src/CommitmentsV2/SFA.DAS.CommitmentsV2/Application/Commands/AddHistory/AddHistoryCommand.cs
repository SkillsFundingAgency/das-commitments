using System;
using MediatR;
using SFA.DAS.CommitmentsV2.Messages.Events;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddHistory
{
    public class AddHistoryCommand : IRequest
    {
        public Guid CorrelationId { get; set; }
        public UserAction StateChangeType { get; set; }
        public long EntityId { get; set; }
        public string EntityType { get; set; }
        public long EmployerAccountId { get; set; }
        public long ProviderId { get; set; }
        public string InitialState { get; set; }
        public string UpdatedState { get; set; }
        public string Diff { get; set; }
        public string UpdatingUserId { get; set; }
        public string UpdatingUserName { get; set; }
        public Party UpdatingParty { get; set; }
        public DateTime UpdatedOn { get; set; }
        public long? ApprenticeshipId { get; set; }
    }
}
