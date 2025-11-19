using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;

namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateLearner;

public partial class ValidateLearnerCommandHandler
{
    private IEnumerable<LearnerError> ValidateCourseCode(LearnerDataEnhanced record, ProviderStandardResults providerStandardResults)
    {
        var domainErrors = new List<LearnerError>();
        
        if (record.CourseCode.Length > 5)
        {
            domainErrors.Add(new LearnerError("CourseCode", "Enter a valid standard code"));
        }
        else if (GetStandardDetails(record.CourseCode) == null)
        {
            domainErrors.Add(new LearnerError("CourseCode", "Enter a valid standard code"));
        }
        else if (providerStandardResults.IsMainProvider && !IsValidMainProviderStandardDetails(record.CourseCode, providerStandardResults))
        {
            domainErrors.Add(new LearnerError("CourseCode", "Enter a valid standard code. You have not told us that you deliver this training course. You must assign the course to your account in the <a href=" + urlHelper.CourseManagementLink($"12344/review-your-details") + " class='govuk - link'>Your standards and training venues</a> section."));
        }

        return domainErrors;
    }

    private static List<LearnerError> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults)
    {
        var domainErrors = new List<LearnerError>();
        if (providerStandardResults.IsMainProvider && !providerStandardResults.Standards.Any())
        {
            domainErrors.Add(new LearnerError("DeclaredStandards", "No Standards Declared"));
        }

        return domainErrors;
    }

    private static bool IsValidMainProviderStandardDetails(string stdCode, ProviderStandardResults providerStandardResults)
    {
        if (string.IsNullOrWhiteSpace(stdCode)) return false;
        if (providerStandardResults.Standards == null) return false;

        var result = int.Parse(stdCode);

        var standard = providerStandardResults.Standards.FirstOrDefault(x => int.Parse(x.CourseCode) == result);

        return standard != null;
    }
}