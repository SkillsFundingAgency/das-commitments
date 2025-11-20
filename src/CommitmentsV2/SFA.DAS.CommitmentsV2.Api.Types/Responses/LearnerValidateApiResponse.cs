using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class LearnerValidateApiResponse
{
    public List<LearnerError> CriticalErrors { get; set; }
    public LearnerValidation LearnerValidation { get; set; }
}

public class LearnerValidation
{
    public LearnerValidation(long learnerDataId, List<LearnerError> errors)
    {
        LearnerDataId = learnerDataId;
        Errors = errors;
    }

    public long LearnerDataId { get; set; }
    public List<LearnerError>  Errors { get; set; }
}

public class LearnerError
{
    public LearnerError(string property, string error)
    {
        Property = property;
        ErrorText = error;
    }

    public string Property { get; set; }
    public string ErrorText { get; set; }
}