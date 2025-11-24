using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Genrpg.Shared.Utils
{
    public static class XmlUtils
    {
        public static Dictionary<string, string> ExtractAppConfigData(string path)
        {
            string txt = File.ReadAllText(path);

            List<string> lines = txt.Split('\n').ToList();

            Dictionary<string, string> kvDict = new Dictionary<string, string>();

            string prefix = "<add key=\"";
            string suffix = "\"/>";
            string valueStr = "value=\"";
            foreach (string line in lines)
            {
                if (!line.Contains(prefix))
                {
                    continue;
                }
                string newLine = line.Replace(prefix, "").Trim();
                newLine = newLine.Replace(suffix, "").Trim();

                int quoteIndex = newLine.IndexOf("\"");

                if (quoteIndex < 0)
                {
                    continue;
                }

                string key = newLine.Substring(0, quoteIndex).Trim();

                newLine = newLine.Substring(quoteIndex + 1).Trim();

                int valueIndex = newLine.IndexOf(valueStr);

                if (valueIndex < 0)
                {
                    continue;
                }

                string val = newLine.Substring(valueIndex + valueStr.Length).Trim();

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                {
                    kvDict[key] = val;
                }
            }
            return kvDict;
        }
    }
}
