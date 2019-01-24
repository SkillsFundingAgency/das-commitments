using System.Collections.Generic;

namespace SFA.DAS.Commitments.EFCoreTester.Data.Models
{
    public partial class AssessmentOrganisation
    {
        public AssessmentOrganisation()
        {
            Apprenticeship = new HashSet<Apprenticeship>();
        }

        public int Id { get; set; }
        public string EpaorgId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Apprenticeship> Apprenticeship { get; set; }
    }
}
