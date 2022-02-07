using System.Collections.Generic;

namespace SFA.DAS.CommitmentsV2.Types.Dtos
{
    public struct DeliveryModelDto : IEqualityComparer<DeliveryModelDto>
    {
        public DeliveryModelDto(DeliveryModel? dm)
        {
            Code = dm ?? DeliveryModel.Normal;
        }

        public DeliveryModel Code { get; set; }
        public string Description => Code.ToString();

        public override bool Equals(object obj)
        {
            return obj is DeliveryModelDto other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Code * 397);
            }
        }

        public bool Equals(DeliveryModelDto x, DeliveryModelDto y)
        {
            return x.Code == y.Code;
        }

        public int GetHashCode(DeliveryModelDto obj)
        {
            return (int) obj.Code;
        }
    }
}
