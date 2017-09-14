using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using SFA.DAS.Commitments.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.Commitments.Infrastructure.Services
{

    public class AcademicYearValidator : IAcademicYearValidator
    {

        public readonly ICurrentDateTime _currentDateTime;
        public readonly IAcademicYearDateProvider _academicYear;

        public AcademicYearValidator(ICurrentDateTime currentDateTime, IAcademicYearDateProvider academicYear)
        {
            _currentDateTime = currentDateTime;
            _academicYear = academicYear;
        }

        public AcademicYearValidationResult Validate(DateTime startDate)
        {
            if (startDate < _academicYear.CurrentAcademicYearStartDate &&
                 _currentDateTime.Now > _academicYear.LastAcademicYearFundingPeriod)
            {
                return AcademicYearValidationResult.NotWithinFundingPeriod;
            }

            return AcademicYearValidationResult.Success;
        }
    }
}
