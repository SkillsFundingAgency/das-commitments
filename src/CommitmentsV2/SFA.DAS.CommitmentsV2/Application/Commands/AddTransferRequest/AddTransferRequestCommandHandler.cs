using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Data;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Application.Commands.AddTransferRequest
{
    public class AddTransferRequestCommandHandler : IRequestHandler<AddTransferRequestCommand, AddTransferRequestResult>
    {
        private readonly Lazy<ProviderCommitmentsDbContext> _dbContext;
        private readonly ILogger<AddTransferRequestCommandHandler> _logger;

        public AddTransferRequestCommandHandler(
            Lazy<ProviderCommitmentsDbContext> dbContext,
            ILogger<AddTransferRequestCommandHandler> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task<AddTransferRequestResult> Handle(AddTransferRequestCommand request, CancellationToken cancellationToken)
        {
            var db = _dbContext.Value;

            try
            {
                var transferRequest = await TransferRequest.CreateForCohort(db, request.CohortId, cancellationToken);
                await db.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Created TransferRequest Id:{transferRequest.Id} CohortId:{request.CohortId}");

                return new AddTransferRequestResult
                {
                    Id = transferRequest.Id
                };
            }
            catch(Exception e)
        }
    }
}