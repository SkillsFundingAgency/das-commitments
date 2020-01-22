using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFA.DAS.Events.Api.Client;
using SFA.DAS.Events.Api.Types;

namespace SFA.DAS.Commitments.Infrastructure.Services
{
    /// <summary>
    /// A fake Events Api Client implementation that does nothing, used to switch off integration with the Api
    /// without making significant changes to the legacy codebase.
    /// </summary>
    public class DummyEventsApiClient : IEventsApi
    {
        public Task CreateApprenticeshipEvent(Events.Api.Types.ApprenticeshipEvent apprenticeshipEvent)
        {
            return Task.CompletedTask;
        }

        public Task BulkCreateApprenticeshipEvent(IList<Events.Api.Types.ApprenticeshipEvent> apprenticeshipEvents)
        {
            return Task.CompletedTask;
        }

        public Task<List<ApprenticeshipEventView>> GetApprenticeshipEventsById(long fromEventId = 0, int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<ApprenticeshipEventView>> GetApprenticeshipEventsByDateRange(DateTime? fromDate = null, DateTime? toDate = null, int pageSize = 1000,
            int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task CreateAgreementEvent(AgreementEvent agreementEvent)
        {
            return Task.CompletedTask;
        }

        public Task<List<AgreementEventView>> GetAgreementEventsById(long fromEventId = 0, int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<AgreementEventView>> GetAgreementEventsByDateRange(DateTime? fromDate = null, DateTime? toDate = null, int pageSize = 1000,
            int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task CreateAccountEvent(AccountEvent accountEvent)
        {
            return Task.CompletedTask;
        }

        public Task<List<AccountEventView>> GetAccountEventsById(long fromEventId = 0, int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<AccountEventView>> GetAccountEventsByDateRange(DateTime? fromDate = null, DateTime? toDate = null, int pageSize = 1000,
            int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task CreateGenericEvent(GenericEvent genericEvent)
        {
            return Task.CompletedTask;
        }

        public Task<List<GenericEvent>> GetGenericEventsById(string eventType, long fromEventId = 0, int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<GenericEvent>> GetGenericEventsByDateRange(string eventType, DateTime? fromDate = null, DateTime? toDate = null,
            int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task<List<GenericEvent>> GetGenericEventsByResourceId(string resourceType, string resourceId, DateTime? fromDate = null,
            DateTime? toDate = null, int pageSize = 1000, int pageNumber = 1)
        {
            throw new NotImplementedException();
        }

        public Task CreateGenericEvent<T>(IGenericEvent<T> @event)
        {
            return Task.CompletedTask;
        }
    }
}
