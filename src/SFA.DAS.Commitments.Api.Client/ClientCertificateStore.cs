using System;
using System.Security.Cryptography.X509Certificates;

namespace SFA.DAS.Commitments.Api.Client
{
    public class ClientCertificateStore : IDisposable
    {
        private readonly X509Store _store;

        public ClientCertificateStore(X509Store store)
        {
            _store = store;
            _store.Open(OpenFlags.ReadOnly);
        }

        public X509Certificate FindCertificateByThumbprint(string thumbprint)
        {
            return _store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false)[0];
        }

        public void Dispose()
        {
            _store.Close();
        }
    }
}