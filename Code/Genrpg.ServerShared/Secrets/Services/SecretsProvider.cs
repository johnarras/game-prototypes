using Genrpg.ServerShared.Config;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Secrets.Services
{

    public interface ISecretsProvider : IInitializable
    {
        Task<string> GetSecret(string key);
    }



    public class SecretsProvider : ISecretsProvider
    {
        private IServerConfig _serverConfig = null;
        public async Task Initialize(CancellationToken token)
        {
            await Task.CompletedTask;
        }

        public async Task<string> GetSecret(string key)
        {
            await Task.CompletedTask;

            // Replace this with some kind of secure vault.
            return _serverConfig.GetSecret(key);
        }
    }
}
