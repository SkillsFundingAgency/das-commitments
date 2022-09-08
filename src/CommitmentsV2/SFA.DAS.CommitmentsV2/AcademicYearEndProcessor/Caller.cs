namespace SFA.DAS.CommitmentsV2.Domain
{
    public class Caller
    {
        public Caller() { }

        public Caller(long id, CallerType type)
        {
            Id = id;
            CallerType = type;
        }

        public long  Id { get; set; }
        public CallerType CallerType { get; set; }
    }
}