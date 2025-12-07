using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFA.DAS.CommitmentsV2.Domain.Entities;

public class ViewEditDraftApprenticeshipEmailValidationRequest
{
    public long DraftApprenticeshipId { get; set; }
    public string Email { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public long CohortId { get; set; }
}
