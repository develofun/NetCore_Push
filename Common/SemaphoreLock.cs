using System;
using System.Threading;
using System.Threading.Tasks;

namespace NetCore_PushServer
{
    public class SemaphoreLock
    {
        private delegate _SemaphoreLock _Lock();
        private delegate Task<_SemaphoreLock> _LockAsync();
        private _Lock _lock;
        private _LockAsync _lockAsync;
        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1, 1);
        private CancellationToken _ct = CancellationToken.None;
        private TimeSpan _timeout = TimeSpan.Zero;

        public SemaphoreLock()
        {
            _lock = Lock1;
            _lockAsync = Lock1Async;
        }

        public SemaphoreLock(CancellationToken ct)
        {
            _ct = ct;
            _lock = Lock2;
            _lockAsync = Lock2Async;
        }

        public SemaphoreLock(TimeSpan timeout, CancellationToken ct)
        {
            _timeout = timeout;
            _ct = ct;
            _lock = Lock2;
            _lockAsync = Lock2Async;
        }

        public _SemaphoreLock Lock()
        {
            return _lock();
        }

        public async Task<_SemaphoreLock> LockAsync()
        {
            return await _lockAsync();
        }

        private _SemaphoreLock Lock1()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            semaphore.Lock();
            return semaphore;
        }

        private async Task<_SemaphoreLock> Lock1Async()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            await semaphore.LockAsync();
            return semaphore;
        }

        private _SemaphoreLock Lock2()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            semaphore.Lock(_ct);
            return semaphore;
        }

        private async Task<_SemaphoreLock> Lock2Async()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            await semaphore.LockAsync(_ct);
            return semaphore;
        }

        private _SemaphoreLock Lock3()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            semaphore.Lock(_timeout, _ct);
            return semaphore;
        }

        private async Task<_SemaphoreLock> Lock3Async()
        {
            var semaphore = new _SemaphoreLock(_semaphoreSlim);
            await semaphore.LockAsync(_timeout, _ct);
            return semaphore;
        }
    }

    public class _SemaphoreLock : IDisposable
    {
        private bool _isDisposed = false;
        private SemaphoreSlim _semaphoreSlim;

        public _SemaphoreLock(SemaphoreSlim semaphoreSlim)
        {
            _semaphoreSlim = semaphoreSlim;
        }

        ~_SemaphoreLock()
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
                if (_semaphoreSlim != null) _semaphoreSlim.Release();
            }

            _isDisposed = true;
        }

        internal void Lock()
        {
            _semaphoreSlim.Wait();
        }

        internal void Lock(CancellationToken ct)
        {
            try
            {
                _semaphoreSlim.Wait(ct);
            }
            catch
            {
                _semaphoreSlim = null;
                throw;
            }
        }

        internal void Lock(TimeSpan timeout, CancellationToken ct)
        {
            try
            {
                if (_semaphoreSlim.Wait(timeout, ct) == false)
                {
                    _semaphoreSlim = null;
                    throw new TimeoutException("Time out semaphoreLock");
                }
            }
            catch
            {
                _semaphoreSlim = null;
                throw;
            }
        }

        internal async Task LockAsync()
        {
            await _semaphoreSlim.WaitAsync();
        }

        internal async Task LockAsync(CancellationToken ct)
        {
            try
            {
                await _semaphoreSlim.WaitAsync(ct);
            }
            catch
            {
                _semaphoreSlim = null;
                throw;
            }
        }

        internal async Task LockAsync(TimeSpan timeout, CancellationToken ct)
        {
            try
            {
                if (await _semaphoreSlim.WaitAsync(timeout, ct) == false)
                {
                    _semaphoreSlim = null;
                    throw new TimeoutException("Time out semaphoreLock");
                }
            }
            catch
            {
                _semaphoreSlim = null;
                throw;
            }
        }
    }
}
