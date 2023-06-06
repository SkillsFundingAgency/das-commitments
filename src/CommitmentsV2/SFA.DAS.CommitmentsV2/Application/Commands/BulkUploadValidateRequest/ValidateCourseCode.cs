using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateCourseCode(BulkUploadAddDraftApprenticeshipRequest csvRecord, ProviderStandardResults providerStandardResults)
        {
            var domainErrors = new List<Error>();
            if (string.IsNullOrEmpty(csvRecord.CourseCode))
            {
                domainErrors.Add(new Error("CourseCode", "<b>Standard code</b> must be entered"));
            }
            else if (!csvRecord.CourseCode.All(char.IsDigit) && !int.TryParse(csvRecord.CourseCode, out _))
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }
            else if (csvRecord.CourseCode.Length > 5)
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }
            else if (GetStandardDetails(csvRecord.CourseCode) == null)
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code</b>"));
            }
            else if (providerStandardResults.IsMainProvider && !IsValidMainProviderStandardDetails(csvRecord.CourseCode, providerStandardResults))
            {
                domainErrors.Add(new Error("CourseCode", "Enter a valid <b>standard code.</b> You have not told us that you deliver this training course. You must assign the course to your account in the <a href=" + _urlHelper.CourseManagementLink($"{csvRecord.ProviderId}/review-your-details") + " class='govuk - link'>Your standards and training venues</a> section."));

            }

            return domainErrors;
        }

        private List<Error> ValidateDeclaredStandards(ProviderStandardResults providerStandardResults)
        {
            var domainErrors = new List<Error>();
            if (providerStandardResults.IsMainProvider && providerStandardResults.Standards == null)
            {
                domainErrors.Add(new Error("DeclaredStandards", "No Standards Declared"));

            }

            return domainErrors;
        }

        private bool IsValidMainProviderStandardDetails(string stdCode, ProviderStandardResults providerStandardResults)
        {
            if (string.IsNullOrWhiteSpace(stdCode)) return false;
            if (providerStandardResults.Standards == null) return false;

            int.TryParse(stdCode, out int result);

            var standard = providerStandardResults.Standards
                .Where(x => int.Parse(x.CourseCode) == result).FirstOrDefault();

            return standard != null;
        }
    }
}
