using SFA.DAS.CommitmentsV2.Types;

namespace SFA.DAS.Commitments.Support.SubSite.Extensions
{
    public static class DeliveryModelExtension
    {
        public static string ToDescription(this DeliveryModel? deliveryModel) =>
            deliveryModel?.ToDescription();

        public static string ToDescription(this DeliveryModel deliveryModel) =>
            deliveryModel switch
            {
                DeliveryModel.PortableFlexiJob => "Portable flexi-job",
                _ => "Regular"
            };

        public static string ToIrregularDescription(this DeliveryModel deliveryModel) =>
            deliveryModel switch
            {
                DeliveryModel.PortableFlexiJob => "Portable flexi-job",
                _ => null,
            };
    }
}
