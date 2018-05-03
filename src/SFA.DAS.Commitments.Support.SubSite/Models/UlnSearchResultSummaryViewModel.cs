using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class UlnSearchResultSummaryViewModel
    {
        public string Uln { get; set; }

        public int ApprenticeshipsCount { get; set; }

        public List<UlnSearchResultViewModel> SearchResults { get; set; }

        public IEnumerable<string> ErrorMessages { get; set; }

        public bool HasError  => (ErrorMessages != null && ErrorMessages.Any());

    }
}