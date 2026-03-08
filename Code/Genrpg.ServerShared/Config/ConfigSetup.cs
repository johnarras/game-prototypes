using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.DataGroups;
using Microsoft.Azure.Cosmos.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.ServerShared.Config
{
    public class ConfigSetup
    {
        public async Task<IServerConfig> SetupServerConfig(CancellationToken token, string serverId, string envOverride)
        {

            ServerConfig serverConfig = new ServerConfig();

            serverConfig.DefaultEnv = await GetValue(AppConfigKeys.DefaultEnv, null);

            if (!string.IsNullOrEmpty(envOverride))
            {
                serverConfig.DefaultEnv = envOverride;
            }

            serverConfig.ServerId = serverId;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            DefaultAzureCredential credential = new DefaultAzureCredential();

            SecretClient secretsClient = null;

            string secretsVaultURI = await GetValue(AppConfigKeys.KeyVaultURI, null);
            string secretsVaultPrefix = await GetValue(AppConfigKeys.KeyVaultPrefix, null);

            if (!string.IsNullOrEmpty(secretsVaultURI))
            {

                List<string> vaultNames = new List<string>();

                if (!EnvNames.IsProdEnv(serverConfig.DefaultEnv))
                {
                    vaultNames.Add(EnvNames.Dev);
                }
                else
                {
                    vaultNames.Add(EnvNames.Prod + "-write");
                    vaultNames.Add(EnvNames.Prod + "-read");
                }

                SecretClient secretClient = null;
                foreach (string vaultName in vaultNames)
                {
                    if (secretClient == null)
                    {
                        try
                        {
                            string fullSecretsVaultURI = secretsVaultURI.Replace(AppConfigKeys.PlaceholderString,  secretsVaultPrefix + vaultName);
                            secretsClient = new SecretClient(new Uri(fullSecretsVaultURI), credential);
                            await secretsClient.GetPropertiesOfSecretsAsync().AnyAsync();
                            break;
                        }
                        catch
                        {
                            secretsClient = null;
                        }
                    }
                }
            }
            string filePath = config.FilePath;

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                serverConfig.DataEnvs[dataCategory] = await GetValueOrDefault(dataCategory + AppConfigKeys.EnvSuffix, serverConfig.DefaultEnv, secretsClient);
            }

            serverConfig.MessagingEnv = await GetValueOrDefault(AppConfigKeys.MessagingEnv, serverConfig.DefaultEnv, secretsClient);

            serverConfig.ContentRoot = await GetValue(AppConfigKeys.ContentRoot, secretsClient);

            serverConfig.PublicIP = await GetValue(AppConfigKeys.PublicIP, secretsClient);

            serverConfig.PackageName = await GetValue(AppConfigKeys.PackageName, secretsClient);

            serverConfig.IOSBuyValidationURL = await GetValue(AppConfigKeys.IOSBuyValidationURL, secretsClient);

            serverConfig.IOSSandboxValidationURL = await GetValue(AppConfigKeys.IOSSandboxValidationURL, secretsClient);

            serverConfig.GooglePlayValidationURL = await GetValue(AppConfigKeys.GooglePlayValidationURL, secretsClient) ;

            List<string> allKeys = ConfigurationManager.AppSettings.AllKeys.ToList();

            Dictionary<string, string> defaultConnections = new Dictionary<string, string>();

            foreach (string repoType in Enum.GetNames(typeof(ERepoTypes)))
            {
                defaultConnections[repoType] = await GetValue(repoType + AppConfigKeys.Default + AppConfigKeys.ConnectionSuffix, secretsClient);
            }

            foreach (string key in allKeys)
            {
                if (key.IndexOf(AppConfigKeys.ConnectionSuffix) > 0 || key.IndexOf(AppConfigKeys.SecretInfix) >= 0)
                {
                    string shortKey = key.Replace(AppConfigKeys.ConnectionSuffix, "");

                    string defaultValue = "";

                    foreach (string repoType in defaultConnections.Keys)
                    {
                        if (shortKey.IndexOf(repoType) >= 0)
                        {
                            defaultValue = defaultConnections[repoType];
                            break;
                        }
                    }

                    serverConfig.SetSecret(shortKey, await GetValueOrDefault(key, defaultValue, secretsClient));
                }
            }

            await Task.CompletedTask;
            return serverConfig;
        }

        private async Task<string> GetValue(string key, SecretClient secretClient)
        {
            string val = Environment.GetEnvironmentVariable(key);

            if (string.IsNullOrEmpty(val))
            {
                val = ConfigurationManager.AppSettings[key];
            }

            if (secretClient != null)
            {
                try
                {
                    KeyVaultSecret secret = await secretClient.GetSecretAsync(key);
                    if (secret != null && !string.IsNullOrEmpty(secret.Value))
                    {
                        val = secret.Value;
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
            return val;
        }

        private async Task<string> GetValueOrDefault(string key, string defaultValue, SecretClient secretClient)
        {
            string configValue = await GetValue(key, secretClient);
            
            if (configValue == AppConfigKeys.Default)
            {
                return defaultValue;
            }

            if (string.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }
            return configValue;
        }
    }
}


