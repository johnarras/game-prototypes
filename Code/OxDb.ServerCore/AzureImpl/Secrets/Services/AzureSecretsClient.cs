using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using OxDb.ServerCore.Constants;
using OxDb.ServerCore.DataStores.Secrets.Entities;
using OxDb.ServerCore.DataStores.Secrets.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;
using System.Diagnostics;

namespace OxDb.ServerCore.AzureImpl.Secrets.Services
{
    public class AzureSecretsClient : ISecretsClient
    {

        const string KeyVaultURI = "https://XXXXX.vault.azure.net/";

        private SecretClient _secretsClient = null;

        public async Task<string> GetSecretAsync(string key)
        {
            if (_secretsClient != null)
            {

                try
                {
                    KeyVaultSecret secret = await _secretsClient.GetSecretAsync(key);
                    if (secret != null && !string.IsNullOrEmpty(secret.Value))
                    {
                        return secret.Value;
                    }
                }
                catch (RequestFailedException rfe) when (rfe.Status == 404)
                {
                    // This is ok..ignore.
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Missing Secret: " + ex.Message);
                }
            }
            return null;
        }

        public async Task Init(InitSecretsClientArgs args)
        {
            string vaultName = null;

            if (!EnvNames.IsProdEnv(args.Env))
            {
                vaultName = EnvNames.Dev;
            }
            else
            {
                vaultName = EnvNames.Prod;
                if (args.ServerName.ToLower() == ServerNames.Editor.ToLower())
                {
                    vaultName += "-read";
                }
                else
                {
                    vaultName += "-write";
                }
            }

            string fullSecretsVaultURI = KeyVaultURI.Replace(AppConfigKeys.PlaceholderString, args.VaultPrefix + vaultName);

            try
            {
                DefaultAzureCredential cred = new DefaultAzureCredential();
                _secretsClient = new SecretClient(new Uri(fullSecretsVaultURI), cred);
                if (!await _secretsClient.GetPropertiesOfSecretsAsync().AnyAsync())
                {
                    _secretsClient = null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Could not create secrets client for " + args.ServerName + " in env " + args.Env);
            }
        }
    }
}
