using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace OxDb.SharedCore.Utils
{

    public class XmlDict
    {

        public string this[string key]
        {
            get
            {
                return _dict[key];
            }
            set
            {
                _dict[key] = value;  
            }
        }

        private Dictionary<string, string> _dict { get; set; } = new Dictionary<string, string>();


        public XmlDict(Dictionary<string,string> dict)
        {
            _dict = dict;
        }

        public string GetVal(string key)
        {
            return _dict[key];
        }


        public int GetInt(string key)
        {
            return int.Parse(GetVal(key));
        }

        public bool GetBool(string key)
        {
            return bool.Parse(GetVal(key));
        }

        public T GetEnum<T>(string key) where T : Enum
        {
            return (T)Enum.Parse(typeof(T), GetVal(key));
        }
    }


    public static class XmlUtils
    {
        public static XmlDict ExtractAppConfigData(string path)
        {
            string txt = File.ReadAllText(path);

            List<string> lines = StrUtils.SplitIntoLines(txt);

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


                if (val.IndexOf("&amp;") >= 0)
                {
                    Console.WriteLine("Got val");
                }
                val = val.Replace("&amp;", "&");

                if (!string.IsNullOrEmpty(key) && !string.IsNullOrEmpty(val))
                {
                    kvDict[key] = val;
                }
            }
            

            return new XmlDict(kvDict);
        }
    }
}


