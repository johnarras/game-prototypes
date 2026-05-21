using OxDb.SharedCore.Interfaces;

namespace OxDb.ServerCore.Crypto.Services
{
    public interface ICryptoService : IInjectable
    {
        string GetPasswordHash(string salt, string passwordOrToken);
        string GetRandomByteString(int length);
        string SlowHash(string txt);
        byte[] GetRandomBytes(int length);
    }
}


