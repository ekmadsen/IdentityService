using System;


namespace ErikTheCoder.Identity.Service
{
    public interface ISafeRandom : IDisposable
    {
        void NextBytes(byte[] Bytes);
    }
}
