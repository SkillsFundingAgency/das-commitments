﻿using SFA.DAS.Commitments.Support.SubSite.Enums;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class ApprenticeshipSearchQuery
    {
        public string SearchTerm { get; set; }
        public ApprenticeshipSearchType SearchType { get; set; }

        public IEnumerable<string> ReponseMessages { get; set; }
        public string ResponseUrl { get; set; }
        public string HashedAccountId { get; set; }
    }
}