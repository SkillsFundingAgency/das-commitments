using SFA.DAS.Commitments.Domain.Entities.AcademicYear;
using System;

namespace SFA.DAS.Commitments.Domain.Interfaces
{
    public interface IAcademicYearValidator
    {
        AcademicYearValidationResult Validate(DateTime trainingStartDate);
    }
}
