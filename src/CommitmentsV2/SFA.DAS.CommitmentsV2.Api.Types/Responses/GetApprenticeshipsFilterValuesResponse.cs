﻿using System;
using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Api.Types.Responses;

public class GetApprenticeshipsFilterValuesResponse
{
    public IEnumerable<string> EmployerNames { get; set; }
    public IEnumerable<string> ProviderNames { get; set; }
    public IEnumerable<string> CourseNames { get; set; }
    public IEnumerable<string> Statuses { get; set; }
    public IEnumerable<DateTime> StartDates { get; set; }
    public IEnumerable<DateTime> EndDates { get; set; }
}