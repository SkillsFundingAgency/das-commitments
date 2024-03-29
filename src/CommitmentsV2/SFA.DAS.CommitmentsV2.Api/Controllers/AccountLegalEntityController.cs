﻿using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SFA.DAS.CommitmentsV2.Api.Authorization;
using SFA.DAS.CommitmentsV2.Api.Extensions;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Application.Queries.GetAccountLegalEntity;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountLegalEntityController : ControllerBase
    {
        private readonly ILogger<AccountLegalEntityController> _logger;
        private readonly IMediator _mediator;

        public AccountLegalEntityController(
            ILogger<AccountLegalEntityController> logger, 
            IMediator mediator)
        {
            _logger = logger;
            _mediator = mediator;
        }

        [Authorize]
        [HttpGet]
        [Route("{AccountLegalEntityId}")]
        public async Task<IActionResult> GetAccountLegalEntity(long accountLegalEntityId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState.CreateErrorResponse());
            }

            var employer = await _mediator.Send(new GetAccountLegalEntityQuery
            {
                AccountLegalEntityId = accountLegalEntityId
            });

            if (employer == null)
            {
                return NotFound();
            }

            return Ok(new AccountLegalEntityResponse
            {
                AccountId = employer.AccountId,
                MaLegalEntityId = employer.MaLegalEntityId,
                AccountName = employer.AccountName,
                LegalEntityName = employer.LegalEntityName,
                LevyStatus = employer.LevyStatus
            });
        }
    }
}
