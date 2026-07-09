using Assets.Scripts.Config;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Utils;

namespace Assets.Scripts.Logalytics.Utils
{
    public static class LogalyticsUtils
    {
        public static string GetLogConnectionString(ClientConfig config)
        {
            return GetLogalyticsConnectionString(config);
        }

        public static string GetAnalyticsConnectionString(ClientConfig config)
        {
            return GetLogalyticsConnectionString(config);
        }

        private static string GetLogalyticsConnectionString(ClientConfig config)
        {

            // This is a bit strange but it keeps the connection string out of the client until a player is built.
#if UNITY_EDITOR
            XmlDict kvDict = XmlUtils.ExtractAppConfigData(ConfigConstants.MainAppConfigPath);
            string connectionString = kvDict[AppConfigKeys.AppInsightsConnectionString];
            if (!string.IsNullOrEmpty(connectionString))
            {
                return connectionString;
            }
#endif
            return config.LogalyticsConnectionString;

        }
    }
}
