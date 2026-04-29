namespace SFA.DAS.CommitmentsV2.Application.Commands.CocDelete;

public class CocDeleteCommand : IRequest<CocDeleteResult>
{
    public Guid LearningKey { get; set; }
}