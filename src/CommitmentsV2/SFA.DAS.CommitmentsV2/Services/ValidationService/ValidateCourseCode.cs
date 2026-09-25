using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using SFA.DAS.CommitmentsV2.Models;

namespace SFA.DAS.CommitmentsV2.Services.ValidationService;

public partial class ValidationService
{
    public IEnumerable<Error> ValidateCourseCode(string courseCode, ProviderStandardResults providerStandardResults, Standard standard)
    {
        var domainErrors = new List<Error>();
        if (string.IsNullOrEmpty(courseCode))
        {
            domainErrors.Add(new Error("CourseCode", "<b>Standard code</b> must be entered"));
        }
        else if (!courseCode.All(char.IsDigit) && !int.TryParse(courseCode, out _))
        {
            domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>. Apprenticeship units must be added by ILR upload"));
        }
        else if (courseCode.Length > 5)
        {
            domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
        }
        else if (standard == null)
        {
            domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
        }
        else if (providerStandardResults.IsMainProvider && !IsValidMainProviderStandardDetails(courseCode, providerStandardResults))
        {
            domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code.</b> You have not told us that you deliver this training course. You must assign the course to your account in the <a href=" + urlHelper.CourseManagementLink($"{csvRecord.ProviderId}/review-your-details") + " class='govuk - link'>Your standards and training venues</a> section."));
        }

        return domainErrors;
    }

    public List<Error> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults)
    {
        var domainErrors = new List<Error>();
        if (providerStandardResults.IsMainProvider && !providerStandardResults.Standards.Any())
        {
            domainErrors.Add(new Error("DeclaredStandards", "No Standards Declared"));
        }

        return domainErrors;
    }

    private bool IsValidMainProviderStandardDetails(string stdCode, ProviderStandardResults providerStandardResults)
    {
        if (string.IsNullOrWhiteSpace(stdCode)) return false;
        if (providerStandardResults.Standards == null) return false;

        var result = int.Parse(stdCode);

        var standard = providerStandardResults.Standards.FirstOrDefault(x => int.Parse(x.CourseCode) == result);

        return standard != null;
    }
}