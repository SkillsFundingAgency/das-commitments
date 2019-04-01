﻿using System;
using SFA.DAS.CommitmentsV2.Domain.Interfaces;

namespace SFA.DAS.CommitmentsV2.Services
{
    public class AcademicYearDateProvider : IAcademicYearDateProvider
    {
        private readonly ICurrentDateTime _currentDateTime;

        public AcademicYearDateProvider(ICurrentDateTime currentDateTime)
        {
            _currentDateTime = currentDateTime;
        }

        public DateTime CurrentAcademicYearStartDate
        {
            get
            {
                var now = _currentDateTime.Now;
                var cutoff = new DateTime(now.Year, 8, 1);
                return now >= cutoff ? cutoff : new DateTime(now.Year - 1, 8, 1);
            }
        }

        public DateTime CurrentAcademicYearEndDate => CurrentAcademicYearStartDate.AddYears(1).AddDays(-1);

        public DateTime LastAcademicYearFundingPeriod => new DateTime(CurrentAcademicYearStartDate.Year, 10, 19, 18, 0, 0);

    }
}
