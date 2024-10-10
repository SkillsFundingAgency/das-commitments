using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Validation;

public class CommitmentsApiModelException : Exception
{
    public List<ErrorDetail> Errors { get; }

    public CommitmentsApiModelException(List<ErrorDetail> errors) : base("Validation Exception")
    {
        Errors = errors;
    }
}