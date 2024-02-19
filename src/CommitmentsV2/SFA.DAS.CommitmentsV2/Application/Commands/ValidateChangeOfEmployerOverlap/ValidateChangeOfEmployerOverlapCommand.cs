namespace SFA.DAS.CommitmentsV2.Application.Commands.ValidateChangeOfEmployerOverlap
{
    public class ValidateChangeOfEmployerOverlapCommand : IRequest
    {
        public string Uln { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public long ProviderId { get; set; }
    }
}
