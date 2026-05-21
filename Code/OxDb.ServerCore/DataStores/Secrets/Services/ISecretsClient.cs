using OxDb.ServerCore.DataStores.Secrets.Entities;

namespace OxDb.ServerCore.DataStores.Secrets.Services
{



    /// <summary>
    /// This is not meant to go into DI. It's supposed to be used once during init.
    /// </summary>
    public interface ISecretsClient
    {
        Task Init(InitSecretsClientArgs args);
        Task<string> GetSecretAsync(string key);
    }
}
