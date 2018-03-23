using System;
using System.Collections.Generic;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class UlnSearchResultSummaryViewModel
    {
        public string Uln { get; set; }

        public int ApprenticeshipsCount { get; set; }

        public List<UlnSearchResult> SearchResults { get; set; }

    }
}