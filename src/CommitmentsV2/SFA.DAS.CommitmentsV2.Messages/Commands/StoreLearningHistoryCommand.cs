using System;
using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.CommitmentsV2.Messages.Commands;

public class StoreLearningHistoryCommand
{
    public long ApprenticeshipId { get; set; }

    public LearningSourceType Source { get; set; }

    public LearningChangeType ChangeType { get; set; }

    public Guid? LearningKey { get; set; }

    public DateTime AppliedDate { get; set; }

    public string Description { get; set; }

    public Guid? UserId { get; set; }
}