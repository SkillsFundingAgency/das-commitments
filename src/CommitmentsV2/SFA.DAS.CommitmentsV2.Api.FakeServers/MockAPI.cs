using System;
using WireMock.Server;

namespace SFA.DAS.CommitmentsV2.Api.FakeServers
{
    public class MockApi : IDisposable
    {
        private readonly WireMockServer _server;

        private bool _isDisposed;

        public MockApi(WireMockServer server)
        {
            _server = server;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed) return;

            if (disposing)
            {
                if (_server != null && _server.IsStarted)
                    _server.Stop();

                _server?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
