using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System;
using System.Collections.Generic;
using System.Text;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation
{
    public class CommitmentsApiBulkUploadModelException : Exception
    {
        public List<BulkUploadValidationError> Errors { get; }

        public CommitmentsApiBulkUploadModelException(List<BulkUploadValidationError> errors) : base("Bulkupload Validation Exception")
        {
            Errors = errors;
        }
    }
}
