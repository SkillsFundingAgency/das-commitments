using System;
using System.Collections.Generic;
using System.Linq;

namespace SFA.DAS.Commitments.Support.SubSite.Models
{
    public class UlnSearchResultSummaryViewModel
    {

        public UlnSearchResultSummaryViewModel()
        {
            ReponseMessages = new List<string>();
        }

        public string Uln { get; set; }

        public int ApprenticeshipsCount { get; set; }

        public List<UlnSearchResultViewModel> SearchResults { get; set; }

        public List<string> ReponseMessages { get; set; }

        public bool HasError => ReponseMessages != null && ReponseMessages.Any();

    }
}