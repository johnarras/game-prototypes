using Genrpg.Shared.MapMessages.Interfaces;
using Genrpg.Shared.Movement.Messages;
using Genrpg.Shared.Spells.Messages;
using Genrpg.Shared.Stats.Messages;
using MessagePack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Genrpg.Shared.MapMessages
{

    class MapApiTypeSettings
    {
        public Type InterfaceType { get; set; }
        public Type BaseClassType { get; set; }
        // 0 = toplevel, N+1 is subinterface. Recommend keeping it linear and to 1-2 levels tops.
        // Used to make the most frequent messagepack messages only use one byte for message type.
        // This is like the idea behind Huffman encoding. More bytes for less frequent symbols/messages.
        public int Order { get; set; } 
        public string FilePath { get; set; }
        public List<string> FullMessageTypeNames { get; set; } = new List<string>();
        public string InterfaceTypeName => InterfaceType.Name;
        public string BaseTypeName => BaseClassType.Name; 

        public string InterfaceDeclarationInCode => "public interface " + InterfaceTypeName;
    }

    [MessagePackObject]
     public class MapMessageInit
    {
        const string ClassAttribute = "[MessagePackObject]";
        const string KeyPrefix = "[Key(";
        const string KeySuffix = ")]";
        const string UsingText = "using MessagePack;";

        const string UnionPrefix = "[Union(";
        const string UnionMiddle = ",typeof(";
        const string UnionSuffix = "))]";

        const string IgnoreTypeMessage = "MessagePackIgnore";

        const string FullTypePrefixIgnore = "Genrpg.Shared.";

        internal class MessageInitData
        {
            public List<MapApiTypeSettings> MapTypeSettings = new List<MapApiTypeSettings>();

            public MessageInitData()
            {
            }

            public Dictionary<string, string> FullTypeNames { get; set; } = new Dictionary<string, string>();
        }

        public static void InitMapMessages(string dirName)
        {
            MessageInitData data = new MessageInitData();

            data.MapTypeSettings = new List<MapApiTypeSettings>();
            data.MapTypeSettings.Add(new MapApiTypeSettings()
            {
                InterfaceType = typeof(IMapApiMessage),
                BaseClassType = typeof(BaseMapApiMessage),
                Order = 0,
            });
            data.MapTypeSettings.Add(new MapApiTypeSettings()
            {
                InterfaceType = typeof(IInfrequentMapApiMessage),
                BaseClassType = typeof(BaseInfrequenMapApiMessage),
                Order = 1,
            });


            data.MapTypeSettings = data.MapTypeSettings.OrderByDescending(x => x.Order).ToList();

            Assembly assembly = Assembly.GetAssembly(typeof(MapMessageInit));

            foreach (var type in assembly.GetTypes())
            {
                data.FullTypeNames[type.Name] = type.FullName.Replace(FullTypePrefixIgnore, "");
            }

            dirName += "Genrpg.Shared";

            List<string> mapTypeNames = new List<string>();

            AddDirectoryFiles(dirName, data);

            foreach (MapApiTypeSettings settings in data.MapTypeSettings)
            {
                if (string.IsNullOrEmpty(settings.FilePath))
                {
                    continue;
                }

                string txt = File.ReadAllText(settings.FilePath);
                List<string> lines = txt.Split("\n").ToList();

                bool foundUsing = false;
                foreach (string line in lines)
                {
                    if (line.Contains(UsingText))
                    {
                        foundUsing = true;
                        break;
                    }
                }

                if (!foundUsing)
                {
                    lines.Insert(0, UsingText);
                }


                lines = lines.Where(x => x.IndexOf(UnionPrefix) < 0).ToList();

                int interfaceLine = -1;
                for (int lid  = 0; lid < lines.Count; lid++)
                {

                    if (lines[lid].Contains(settings.InterfaceDeclarationInCode))
                    {
                        interfaceLine = lid;
                        break;
                    }
                }

                int unionIndex = 0;

                //MapApiTypeSettings nextSettings = data.MapTypeSettings.FirstOrDefault(x => x.Order == settings.Order + 1);
                //if (nextSettings != null)
                //{
                //    lines.Insert(interfaceLine + unionIndex, GetUnionText(nextSettings.InterfaceTypeName, unionIndex));
                //    unionIndex++;
                //}

                foreach (string line in settings.FullMessageTypeNames)
                { 
                    lines.Insert(interfaceLine + unionIndex, GetUnionText(line, unionIndex));
                    unionIndex++;
                }

                StringBuilder sb = new StringBuilder();

                for (int lid = 0; lid < lines.Count; lid++)
                {
                    string line = lines[lid];
                    if (lid == lines.Count - 1 && string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    sb.Append(line + "\n");
                }

                File.WriteAllText(settings.FilePath, sb.ToString());
            }
        }

        private static string GetUnionText(string typeName, int unionIndex) 
        {
            return "    " + UnionPrefix + unionIndex + UnionMiddle + typeName + UnionSuffix;
        }

        private static void AddDirectoryFiles(string path, MessageInitData data)
        {
            try
            {
                string[] files = Directory.GetFiles(path);

                foreach (var file in files)
                {
                    if (file.IndexOf(".cs") != file.Length - 3)
                    {
                        continue;
                    }

                    if (file.IndexOf("MapMessageInit") >= 0)
                    {
                        continue;
                    }

                    string fullPath = Path.Combine(new string[] { path, file });

                    string txt = File.ReadAllText(fullPath);

                    List<string> lines = txt.Split("\n").ToList();

                    bool foundUsing = false;
                    bool addedAnyClass = false;

                    int startKeyIndex = 0;

                    int classIndex = 0;

                    bool ignoreType = false;
                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        if (lines[lid].Contains(IgnoreTypeMessage))
                        {
                            ignoreType = true;
                            break;
                        }

                        if (lines[lid].Contains("public class") || lines[lid].Contains("public sealed class"))
                        {
                            lines[lid] = lines[lid].Replace(ClassAttribute + " ", "");
                        }
                        else if (lines[lid].Contains(ClassAttribute))
                        {
                            lines.RemoveAt(lid);
                            lid--;
                        }
                    }

                    if (ignoreType)
                    {
                        continue;
                    }

                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        string line = lines[lid];

                        classIndex = lid;

                        foreach (MapApiTypeSettings settings in data.MapTypeSettings)
                        {
                            if (line.IndexOf(settings.InterfaceDeclarationInCode) >= 0)
                            {
                                settings.FilePath = fullPath;
                                break;
                            }
                        }

                        if (line.IndexOf(UsingText) >= 0)
                        {
                            foundUsing = true;
                        }

                        if (line.IndexOf(".Services") >= 0)
                        {
                            break;
                        }

                        if ((line.IndexOf("public class") < 0 && line.IndexOf("public sealed class") < 0) ||
                            line.IndexOf("Helper") >= 0)
                        {
                            continue;
                        }

                        foreach (MapApiTypeSettings settings in data.MapTypeSettings)
                        {
                            if (line.IndexOf(": " + settings.BaseTypeName) > 0 ||
                                line.IndexOf(settings.InterfaceTypeName) > 0)
                            {

                                string[] words = line.Split(' ');

                                if (words.Any(x => x == "abstract"))
                                {
                                    continue;
                                }

                                foreach (string word in words)
                                {
                                    if (!string.IsNullOrEmpty(word) && word != "public" && word != "sealed" && word != "class")
                                    {
                                        if (data.FullTypeNames.ContainsKey(word))
                                        {
                                            settings.FullMessageTypeNames.Add(data.FullTypeNames[word]);
                                            break;
                                        }
                                    }
                                }
                            }
                        }

                        int keyIndex = startKeyIndex;

                        bool shouldAddClass = false;
                        while (++lid < lines.Count)
                        {
                            string line2 = lines[lid];
                            if (line2.IndexOf(" class") >= 0)
                            {
                                lid--;
                                break;
                            }

                            if (line2.IndexOf("[IgnoreMember]") >= 0)
                            {
                                continue;
                            }

                            if (line2.IndexOf("public") < 0 || line2.IndexOf("{ get; set; }") < 0 || line2.IndexOf("const") >= 0 ||
                                line2.IndexOf("static") >= 0)
                            {
                                continue;
                            }

                            if (line2.Contains(KeyPrefix))
                            {
                                line2 = line2.Substring(0, line2.IndexOf(KeyPrefix)) +
                                    line2.Substring(line2.IndexOf(KeySuffix) + 3);
                            }

                            lines[lid] = line2.Replace("public ", KeyPrefix + keyIndex++ + KeySuffix + " public ");
                            shouldAddClass = true;
                            addedAnyClass = true;
                        }

                        if (shouldAddClass)
                        {
                            lines.Insert(classIndex, "    " + ClassAttribute);
                            lid++;
                        }
                    }

                    if (addedAnyClass && !foundUsing)
                    {
                        lines.Insert(0, UsingText);
                    }

                    StringBuilder newTxt = new StringBuilder();

                    for (int lid = 0; lid < lines.Count; lid++)
                    {
                        string line = lines[lid];

                        if (lid == lines.Count-1 && string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        newTxt.Append(line + "\n");
                    }

                    if (addedAnyClass)
                    {
                        File.WriteAllText(fullPath, newTxt.ToString());
                    }

                }
                List<string> directories = Directory.GetDirectories(path).ToList();
                foreach (string dir in directories)
                {
                    AddDirectoryFiles(dir, data);
                }
            }
            catch (Exception ee)
            {
                Console.WriteLine(ee.Message + " " + ee.StackTrace);
            }
        }
    }
}

