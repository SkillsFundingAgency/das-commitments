using MediatR;
using SFA.DAS.CommitmentsV2.Api.Types.Requests;
using SFA.DAS.CommitmentsV2.Api.Types.Responses;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace SFA.DAS.CommitmentsV2.Application.Commands.BulkUploadValidateRequest
{
    public partial class BulkUploadValidateCommandHandler : IRequestHandler<BulkUploadValidateCommand, BulkUploadValidateApiResponse>
    {
        private List<Error> ValidateDateOfBirth(CsvRecord csvRecord)
        {
            var domainErrors = new List<Error>();
           
            if (string.IsNullOrEmpty(csvRecord.DateOfBirth))
            {
                domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
            }
            else if (!Regex.IsMatch(csvRecord.DateOfBirth, "^\\d\\d\\d\\d-\\d\\d-\\d\\d$"))
            {
                domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));

            }
            else
            {
                var dateOfBith = GetValidDate(csvRecord.DateOfBirth, "yyyy-MM-dd");
                if (dateOfBith == null)
                {
                    domainErrors.Add(new Error("DateOfBirth", "Enter the apprentice's <b>date of birth</b> using the format yyyy-mm-dd, for example 2001-04-23"));
                }
                else if (!WillApprenticeBeAtLeast15AtStartOfTraining(csvRecord.StartDate, dateOfBith.Value))
                {
                    domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are at least 15 years old at the start of their training"));
                }
                else if (!ApprenticeAgeMustBeLessThen115AtStartOfTraining(csvRecord.StartDate, dateOfBith.Value))
                {
                    domainErrors.Add(new Error("DateOfBirth", "The apprentice's <b>date of birth</b> must show that they are not older than 115 years old at the start of their training"));
                }
            }

            return domainErrors;
        }

        private bool WillApprenticeBeAtLeast15AtStartOfTraining(string startDateTime, DateTime dobDate)
        {
            var startDate = GetValidDate(startDateTime, "yyyy-MM-dd");
            if (startDate == null) return true; // Don't fail validation if both fields not set

            int age = startDate.Value.Year - dobDate.Year;
            if (startDate < dobDate.AddYears(age)) age--;

            return age >= 15;
        }

        private bool ApprenticeAgeMustBeLessThen115AtStartOfTraining(string startDateTime, DateTime dobDate)
        {
            var startDate = GetValidDate(startDateTime, "yyyy-MM-dd");
            if (startDate == null) return true; // Don't fail validation if both fields not set

            int age = startDate.Value.Year - dobDate.Year;
            if (startDate < dobDate.AddYears(age)) age--;

            return age < 115;
        }
    }
}
