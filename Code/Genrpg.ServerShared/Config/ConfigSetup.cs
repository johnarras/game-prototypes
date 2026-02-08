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
            serverConfig.DefaultEnv = ConfigurationManager.AppSettings[AppConfigKeys.MainEnv];

            if (!string.IsNullOrEmpty(envOverride))
            {
                serverConfig.DefaultEnv = envOverride;
            }

            serverConfig.ServerId = serverId;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            DefaultAzureCredential credential = new DefaultAzureCredential();

            SecretClient secretsClient = null;

            string secretsVaultURI = ConfigurationManager.AppSettings[AppConfigKeys.KeyVaultURI];

            if (!string.IsNullOrEmpty(secretsVaultURI))
            {
                string secretsSuffix = EnvNames.Dev.ToLower();
                if (serverConfig.DefaultEnv.IndexOf(EnvNames.Prod.ToLower()) == 0 ||
                        serverConfig.DefaultEnv.IndexOf(EnvNames.Staging.ToLower()) == 0)
                {
                    secretsSuffix = EnvNames.Prod.ToLower() + "-testing";
                }

                secretsVaultURI = secretsVaultURI.Replace(AppConfigKeys.PlaceholderString, AppConfigKeys.OrgSecretsVaultPrefix + secretsSuffix);

                try
                {
                    secretsClient = new SecretClient(new Uri(secretsVaultURI), new DefaultAzureCredential());

                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to connect to secrets vault: " + e.Message);
                }
            }
            string filePath = config.FilePath;

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                serverConfig.DataEnvs[dataCategory] = await GetValueOrDefault(dataCategory + AppConfigKeys.EnvSuffix, serverConfig.DefaultEnv, secretsClient);
            }

            serverConfig.MessagingEnv = await GetValueOrDefault(AppConfigKeys.MessagingEnv, serverConfig.DefaultEnv, secretsClient);

            serverConfig.ContentRoot = ConfigurationManager.AppSettings[AppConfigKeys.ContentRoot];

            serverConfig.PublicIP = ConfigurationManager.AppSettings[AppConfigKeys.PublicIP];

            serverConfig.PackageName = ConfigurationManager.AppSettings[AppConfigKeys.PackageName];

            serverConfig.IOSBuyValidationURL = ConfigurationManager.AppSettings[AppConfigKeys.IOSBuyValidationURL];

            serverConfig.IOSSandboxValidationURL = ConfigurationManager.AppSettings[AppConfigKeys.IOSSandboxValidationURL];

            serverConfig.GooglePlayValidationURL = ConfigurationManager.AppSettings[AppConfigKeys.GooglePlayValidationURL];

            List<string> allKeys = ConfigurationManager.AppSettings.AllKeys.ToList();

            Dictionary<string, string> defaultConnections = new Dictionary<string, string>();

            foreach (string repoType in Enum.GetNames(typeof(ERepoTypes)))
            {
                defaultConnections[repoType] = ConfigurationManager.AppSettings[repoType + AppConfigKeys.Default + AppConfigKeys.ConnectionSuffix];
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


            string txt = serverConfig.GetSecret("TestSecret");
            Console.WriteLine("TestSecret:" + txt);
            await Task.CompletedTask;
            return serverConfig;
        }

        private async Task<string> GetValueOrDefault(string key, string defaultValue, SecretClient secretClient)
        {
            string configValue = ConfigurationManager.AppSettings[key];
            
            if (configValue == AppConfigKeys.Default)
            {
                return defaultValue;
            }
            
            if (secretClient != null)
            {
                try
                {
                    KeyVaultSecret secret = await secretClient.GetSecretAsync(key);
                    if (secret != null && !string.IsNullOrEmpty(secret.Value))
                    {
                        configValue = secret.Value;
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


            if (string.IsNullOrEmpty(configValue))
            {
                return defaultValue;
            }
            return configValue;
        }
    }
}


