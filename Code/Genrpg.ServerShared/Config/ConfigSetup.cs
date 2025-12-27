using Genrpg.Shared.Config.Constants;
using Genrpg.Shared.DataStores.DataGroups;
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
        public async Task<IServerConfig> SetupServerConfig(CancellationToken token, string serverId)
        {
            ServerConfig serverConfig = new ServerConfig();
            serverConfig.ServerId = serverId;
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string filePath = config.FilePath;

            serverConfig.DefaultEnv = ConfigurationManager.AppSettings[AppConfigKeys.MainEnv];

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                serverConfig.DataEnvs[dataCategory] = GetValueOrDefault(dataCategory + AppConfigKeys.EnvSuffix, serverConfig.DefaultEnv);
            }

            serverConfig.MessagingEnv = GetValueOrDefault(AppConfigKeys.MessagingEnv, serverConfig.DefaultEnv);

            serverConfig.ContentRoot = ConfigurationManager.AppSettings[AppConfigKeys.ContentRoot];

            serverConfig.PublicIP = ConfigurationManager.AppSettings[AppConfigKeys.PublicIP];

            SetSecret(serverConfig, AppConfigKeys.EtherscanKey);
            SetSecret(serverConfig, AppConfigKeys.IOSSecret);
            SetSecret(serverConfig, AppConfigKeys.GooglePlaySecret);

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
                if (key.IndexOf(AppConfigKeys.ConnectionSuffix) > 0)
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

                    serverConfig.SetSecret(shortKey, GetValueOrDefault(key, defaultValue));
                }
            }

            await Task.CompletedTask;
            return serverConfig;
        }

        private void SetSecret(ServerConfig config, string key)
        {
            config.SetSecret(key, ConfigurationManager.AppSettings[key]);
        }

        private string GetValueOrDefault(string key, string defaultValue)
        {
            string configValue = ConfigurationManager.AppSettings[key];

            if (string.IsNullOrEmpty(configValue) || configValue == AppConfigKeys.Default)
            {
                return defaultValue;
            }
            return configValue;
        }
    }
}


