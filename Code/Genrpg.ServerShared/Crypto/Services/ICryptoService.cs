using Genrpg.Shared.Interfaces;

namespace Genrpg.ServerShared.Crypto.Services
{
    public interface ICryptoService : IInjectable
    {
        string GetPasswordHash(string salt, string passwordOrToken);
        string GetRandomBytes();
        string SlowHash(string txt);
    }
}


