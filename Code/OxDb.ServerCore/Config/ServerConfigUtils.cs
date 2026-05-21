using System.Configuration;

namespace OxDb.ServerCore.Config
{
    public static class ServerConfigUtils
    {
        public static string GetHardcodedConfigValue(string key)
        {

            string val = Environment.GetEnvironmentVariable(key)!;

            if (string.IsNullOrEmpty(val))
            {
                val = ConfigurationManager.AppSettings[key]!;
            }

            return val;
        }

        public static void AddHardCodedValueToDictionary(Dictionary<string, object> dict, string dictKey, string configKey, string defaultValue)
        {
            string value = GetHardcodedConfigValue(dictKey);

            if (!string.IsNullOrEmpty(value))
            {
                dict[dictKey] = value;
            }
        }
    }
}
