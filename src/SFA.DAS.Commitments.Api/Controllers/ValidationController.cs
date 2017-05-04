﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using SFA.DAS.Commitments.Api.Attributes;
using SFA.DAS.Commitments.Api.Orchestrators;
using SFA.DAS.Commitments.Api.Types.Validation;

namespace SFA.DAS.Commitments.Api.Controllers
{
    [RoutePrefix("api/validation")]
    public class ValidationController : ApiController
    {
        private readonly ValidationOrchestrator _validationOrchestrator;

        public ValidationController(ValidationOrchestrator validationOrchestrator)
        {
            if (validationOrchestrator == null)
            {
                throw new ArgumentNullException(nameof(validationOrchestrator));
            }

            _validationOrchestrator = validationOrchestrator;
        }

        [Route("apprenticeships/overlapping")]
        [ApiAuthorize(Roles = "Role1")]
        public async Task<IHttpActionResult> ValidateOverlappingApprenticeships([FromBody]IEnumerable<ApprenticeshipOverlapValidationRequest> request)
        {
            var result = await _validationOrchestrator.ValidateOverlappingApprenticeships(request);
            return Ok(result);
        }
    }
}