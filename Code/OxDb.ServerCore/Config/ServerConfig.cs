using OxDb.ServerCore.AzureImpl.Secrets.Services;
using OxDb.ServerCore.DataStores.Secrets.Entities;
using OxDb.ServerCore.DataStores.Secrets.Services;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.DataStores.DataGroups;
using OxDb.SharedCore.Interfaces;
using System.Configuration;

namespace OxDb.ServerCore.Config
{

    public interface IServerConfig : IInjectable, IExplicitInject
    {
        string GameComponent { get; }
        string Env { get; set; }
        string ProductName { get; }
        string ServerVersion { get; }

        Dictionary<string, string> DataEnvs { get; }
        void ClearSecretsAfterInit();

        string GetConfigVal(string key);
    }

    public class ServerConfig : IServerConfig
    {

        public string GameComponent { get; set; }
        public string Env { get; set; }
        public string ProductName { get; set; }
        public string ServerVersion { get; set; }

        public Dictionary<string, string> DataEnvs { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, string> _configVals { get; set; } = new Dictionary<string, string>();


        public async Task Init<T>(string serverName) where T : ISecretsClient, new()
        {
            Env = await GetValue(AppConfigKeys.DefaultEnv, null);
            ProductName = await GetValue(AppConfigKeys.ProductName, null);
            GameComponent = serverName;
            ServerVersion = await GetValue(AppConfigKeys.ServerVersion, null);

            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            InitSecretsClientArgs args = new InitSecretsClientArgs()
            {
                Env = Env,
                ServerName = serverName,
                VaultPrefix = await GetValue(AppConfigKeys.KeyVaultPrefix, null),
            };

            ISecretsClient secretsClient = new AzureSecretsClient();
            await secretsClient.Init(args);

            string filePath = config.FilePath;

            foreach (string dataCategory in Enum.GetNames(typeof(EDataCategories)))
            {
                DataEnvs[dataCategory] = await GetValueOrDefault(dataCategory + AppConfigKeys.EnvSuffix, Env, secretsClient);
            }

            List<string> allKeys = ConfigurationManager.AppSettings.AllKeys.ToList()!;

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

                    SetConfigVal(shortKey, await GetValueOrDefault(key, defaultValue, secretsClient));
                }
                else
                {
                    SetConfigVal(key, ConfigurationManager.AppSettings[key]!);
                }
            }
        }


        private void SetConfigVal(string key, string val)
        {
            _configVals.Add(key, val);
        }

        public string GetConfigVal(string key)
        {
            if (_configVals.TryGetValue(key, out string value))
            {
                return value;
            }
            return null;
        }

        public void ClearSecretsAfterInit()
        {
            List<string> keys = _configVals.Keys.ToList();

            foreach (string key in keys)
            {
                if (key.IndexOf(AppConfigKeys.ConnectionSuffix) >= 0 ||
                    key.IndexOf(AppConfigKeys.SecretInfix) >= 0)
                {
                    _configVals.Remove(key);
                }
            }
        }

        private async Task<string> GetValue(string key, ISecretsClient secretClient)
        {
            string val = ServerConfigUtils.GetHardcodedConfigValue(key);

            if (secretClient != null)
            {
                string tempVal = await secretClient.GetSecretAsync(key);

                if (!string.IsNullOrEmpty(tempVal))
                {
                    val = tempVal;
                }

            }
            return val;
        }

        private async Task<string> GetValueOrDefault(string key, string defaultValue, ISecretsClient secretClient)
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


