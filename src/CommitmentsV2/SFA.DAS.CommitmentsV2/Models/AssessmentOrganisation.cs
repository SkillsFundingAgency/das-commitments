namespace SFA.DAS.CommitmentsV2.Models;

public partial class AssessmentOrganisation
{
    public AssessmentOrganisation()
    {
        Apprenticeship = new HashSet<ApprenticeshipBase>();
    }

    public int Id { get; set; }
    public string EpaOrgId { get; set; }
    public string Name { get; set; }

    public virtual ICollection<ApprenticeshipBase> Apprenticeship { get; set; }
}