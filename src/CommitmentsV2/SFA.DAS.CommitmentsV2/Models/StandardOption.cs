
namespace SFA.DAS.CommitmentsV2.Models
{
    public class StandardOption
    {
        public string StandardUId { get; set; }
        public string Option { get; set; }

        public virtual Standard Standard { get; set; }
    }
}
