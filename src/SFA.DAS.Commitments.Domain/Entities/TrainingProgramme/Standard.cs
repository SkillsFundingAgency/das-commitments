namespace SFA.DAS.Commitments.Domain.Entities.TrainingProgramme
{
    public class Standard : ITrainingProgramme
    {
        public string Id { get; set; }
        public long Code { get; set; }
        public string Title { get; set; }
        public string CourseName { get; set; }
        public int Level { get; set; }
        public int Duration { get; set; }
        public int MaxFunding { get; set; }
    }
}