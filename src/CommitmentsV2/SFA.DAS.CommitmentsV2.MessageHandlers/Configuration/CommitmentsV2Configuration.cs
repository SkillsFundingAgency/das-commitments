namespace SFA.DAS.CommitmentsV2.MessageHandlers.Configuration
{
    public class CommitmentsV2Configuration
    {
        public string DatabaseConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public string NServiceBusLicense
        {
            get => _decodedNServiceBusLicense ??
                   (_decodedNServiceBusLicense = System.Net.WebUtility.HtmlDecode(_nServiceBusLicense));
            set => _nServiceBusLicense = value;
        }

        private string _nServiceBusLicense;
        private string _decodedNServiceBusLicense;
    }

}
