using SFA.DAS.Http.Configuration;

namespace SFA.DAS.ReservationsV2.Api.Client.DependencyResolution
{
    /// <summary>
    ///     Presents the reservation configuration as an IAzureActiveDirectoryClientConfiguration
    /// </summary>
    public class ReservationsClientApiConfiguration : Reservations.Api.Types.Configuration.ReservationsClientApiConfiguration,
        IAzureActiveDirectoryClientConfiguration
    {
    }
}