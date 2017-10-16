namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public interface ITrainingProgramme
    {
        string Id { get; set; }
        string Title { get; set; }
        int Level { get; set; }
        int MaxFunding { get; set; }


    }
}
