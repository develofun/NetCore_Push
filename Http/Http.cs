using System;
using System.Net.Http;

namespace NetCore_PushServer
{
    public class Http : IDisposable
    {
        protected bool _isDisposed = false;
        protected SemaphoreLock semaphoreLock;

        protected HttpClient _client;

        public Http()
        {
            semaphoreLock = new SemaphoreLock();
            _client = new HttpClient();
        }

        ~Http()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            if (_isDisposed) return;

            if (isDisposing)
            {
                _client.Dispose();
            }

            _isDisposed = true;
        }
    }
}
