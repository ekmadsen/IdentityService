using System;
using System.Security.Cryptography;


namespace ErikTheCoder.Identity.Service
{
    public class SafeRandom : ISafeRandom
    {
        private RNGCryptoServiceProvider _random;
        private object _lock;
        private bool _disposed;


        public SafeRandom()
        {
            _random = new RNGCryptoServiceProvider();
            _lock = new object();
        }


        ~SafeRandom()
        {
            Dispose(false);
        }
        

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        private void Dispose(bool Disposing)
        {
            if (_disposed) return;
            // Release unmanaged resources.
            lock (_lock)
            {
                _random.Dispose();
                _random = null;
            }
            if (Disposing)
            {
                // Release managed resources.
                _lock = null;
            }
            _disposed = true;
        }


        public void NextBytes(byte[] Bytes)
        {
            lock (_lock)
            {
                _random.GetBytes(Bytes);
            }
        }
    }
}
