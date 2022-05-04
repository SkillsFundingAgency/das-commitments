using SFA.DAS.Commitments.Domain.Entities;
using SFA.DAS.Commitments.Support.SubSite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SFA.DAS.Commitments.Support.SubSite.Mappers
{
    public interface ICommitmentMapper
    {

        CommitmentSummaryViewModel MapToCommitmentSummaryViewModel(Commitment commitment);
        CommitmentDetailViewModel MapToCommitmentDetailViewModel(Commitment commitment);

    }
}