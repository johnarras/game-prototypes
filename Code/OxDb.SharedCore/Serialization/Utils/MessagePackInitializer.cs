using OxDb.SharedCore.Serialization.Attributes;
using OxDb.SharedCore.Setup.Services;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OxDb.SharedCore.Serialization.Utils
{

    public class MessagePackInitializer
    {
        readonly Type MainAttribute = typeof(MessagePackInterfaceAttribute);

        readonly List<LinePrefixPair> _attributesToRemove = new List<LinePrefixPair>()
        {
           new LinePrefixPair() { Prefix = MpClassAttribute },
           new LinePrefixPair() { Prefix = MpKeyPrefix, Suffix = MpKeySuffix },
           new LinePrefixPair() { Prefix = MpUnionPrefix },

        };
        const string StartWhitespace = "    ";

        const string MpClassAttribute = "[MessagePackObject]";
        const string MpKeyPrefix = "[Key(";
        const string MpKeySuffix = ")]";

        const string MpUnionPrefix = "[Union(";
        const string MpUnionMiddle = " ,typeof(";
        const string MpUnionSuffix = "))]";

        const string TypePathPrefix = "OxDb.";

        readonly List<string> _neededUsings = new List<string>()
        {
            "MessagePack",
        };



        internal class MessageInitData
        {
            public List<MapApiTypeSettings> MapTypeSettings = new List<MapApiTypeSettings>();

            public MessageInitData()
            {
            }

            public Dictionary<string, string> FullTypeNames { get; set; } = new Dictionary<string, string>();
        }



        private IReflectionService _reflectionService = null;
        public void Init(string dirName, IReflectionService reflectionService, Assembly topLevelAssembly)
        {
            _reflectionService = reflectionService;

            RootClearExistingAttributes(dirName);
            List<Type> allTypes = GetAllTypes(topLevelAssembly);
            List<Type> validInterfaces = GetValidInterfaces(allTypes);

            List<InterfaceTypeList> typeList = GetInterfaceTypeLists(validInterfaces, allTypes);

            List<Type> validClasses = GetValidClasses(typeList, allTypes);

            RootAddClassAttributes(dirName, validClasses);

            AddInterfaceAttributes(dirName, typeList);

        }

        private List<Type> GetAllTypes(Assembly topLevelAssembly)
        {
            List<Assembly> assemblies = _reflectionService.GetSearchAssemblies(topLevelAssembly);

            List<Assembly> checkAssemblies = new List<Assembly>();

            List<Type> allTypes = new List<Type>();

            string[] validAssemblyPrefixes = SetupService.ValidAssemblyPrefixes;

            foreach (Assembly assembly in assemblies)
            {
                foreach (string prefix in validAssemblyPrefixes)
                {
                    if (assembly.GetName().Name.IndexOf(prefix) >= 0)
                    {
                        checkAssemblies.Add(assembly);
                        allTypes.AddRange(assembly.GetExportedTypes());
                        break;
                    }
                }
            }
            return allTypes;
        }

        private List<Type> GetValidClasses(List<InterfaceTypeList> list, List<Type> allTypes)
        {
            List<Type> retval = new List<Type>();

            foreach (InterfaceTypeList itl in list)
            {
                foreach (Type t in itl.DerivedTypes)
                {
                    if (IsValidType(t) && !retval.Contains(t))
                    {

                        retval.Add(t);
                    }
                }
            }


            List<Type> retvalDupe = new List<Type>(retval);

            foreach (Type t in retvalDupe)
            {
                AddExtraTypes(t, retval);
            }

            return retval;
        }

        private void AddExtraTypes(Type currType, List<Type> allTypes)
        {
            PropertyInfo[] props = currType.GetProperties();

            for (int p = 0; p < props.Length; p++)
            {
                Type pt = props[p].PropertyType;
                if (IsValidType(pt) && !allTypes.Contains(pt))
                {
                    allTypes.Add(pt);
                    AddExtraTypes(pt, allTypes);
                }

                if (pt.GenericTypeArguments.Length > 0)
                {
                    foreach (Type gt in pt.GenericTypeArguments)
                    {
                        if (IsValidType(gt) && !allTypes.Contains(gt))
                        {
                            allTypes.Add(gt);
                            AddExtraTypes(gt, allTypes);
                        }
                    }
                }

                if (pt.IsArray)
                {
                    Type atp = pt.GetElementType();
                    if (IsValidType(atp) && !allTypes.Contains(atp))
                    {
                        allTypes.Add(atp);
                        AddExtraTypes(atp, allTypes);
                    }
                }
            }

            if (currType.IsGenericType && currType.GenericTypeArguments.Length > 0)
            {
                foreach (Type gt in currType.GenericTypeArguments)
                {
                    if (IsValidType(gt) && !allTypes.Contains(gt))
                    {
                        allTypes.Add(gt);
                        AddExtraTypes(gt, allTypes);
                    }
                }
            }
        }

        private bool IsValidType(Type t)
        {
            if (!t.IsClass || t.IsAbstract || t.IsInterface || t.IsGenericType ||
                string.IsNullOrEmpty(t.FullName) ||
                t.FullName.IndexOf("Genrpg.Editor") >= 0 ||
                t.FullName.IndexOf(TypePathPrefix) != 0)
            {
                return false;
            }

            MessagePackIgnoreTypeAttribute? attr = t.GetCustomAttribute<MessagePackIgnoreTypeAttribute>(true);

            return attr == null;
        }

        private List<Type> GetValidInterfaces(List<Type> allTypes)
        {

            List<Type> interfacesToSetup = new List<Type>();

            foreach (Type type in allTypes)
            {
                MessagePackInterfaceAttribute? interfaceProp = type.GetCustomAttribute<MessagePackInterfaceAttribute>(true);

                if (interfaceProp != null && type.IsInterface)
                {
                    interfacesToSetup.Add(type);
                }
            }
            return interfacesToSetup;
        }


        private List<InterfaceTypeList> GetInterfaceTypeLists(List<Type> validInterfaces, List<Type> validClasses)
        {
            List<InterfaceTypeList> retval = new List<InterfaceTypeList>();


            foreach (Type type in validInterfaces)
            {
                retval.Add(new InterfaceTypeList() { InterfaceType = type });
            }

            foreach (InterfaceTypeList parentType in retval)
            {
                foreach (InterfaceTypeList childType in retval)
                {
                    if (parentType == childType)
                    {
                        continue;
                    }


                    if (parentType.InterfaceType.IsAssignableFrom(childType.InterfaceType))
                    {
                        parentType.ChildInterfaces.Add(childType.InterfaceType);
                    }
                }
            }

            // Now add classes to child objects.

            foreach (Type cl in validClasses)
            {
                foreach (InterfaceTypeList parentType in retval)
                {
                    if (parentType.InterfaceType.IsAssignableFrom(cl))
                    {
                        bool isChildInterface = false;
                        foreach (Type childInterface in parentType.ChildInterfaces)
                        {
                            if (childInterface.IsAssignableFrom(cl))
                            {
                                isChildInterface = true;
                                break;
                            }
                        }

                        if (!isChildInterface && IsValidType(cl))
                        {
                            parentType.DerivedTypes.Add(cl);
                        }
                    }
                }
            }

            return retval;
        }



        private string GetUnionText(string typeName, int unionIndex)
        {
            return "    " + MpUnionPrefix + unionIndex + MpUnionMiddle + typeName + MpUnionSuffix;
        }



        public void AddInterfaceAttributes(string codeFolder, List<InterfaceTypeList> list)
        {

            foreach (InterfaceTypeList itl in list)
            {
                Type itype = itl.InterfaceType;
                string filePath = itype.FullName;

                filePath = filePath.Replace(TypePathPrefix, "");
                filePath = filePath.Replace(".", "/");
                filePath = TypePathPrefix + filePath;
                filePath += ".cs";

                string fullPath = Path.Combine(codeFolder, filePath);

                string txt = File.ReadAllText(fullPath);

                List<string> lines = StrUtils.SplitIntoLines(txt);

                string lookFor = "public interface " + itype.Name;

                int interfaceLine = -1;


                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l].Contains(lookFor))
                    {
                        interfaceLine = l;
                        break;
                    }
                }

                if (interfaceLine < 0)
                {
                    continue;
                }

                foreach (string usingType in _neededUsings)
                {
                    string usingLine = "using " + usingType + ";";

                    if (!lines.Any(x => x.Contains(usingLine)))
                    {
                        lines.Insert(0, usingLine);
                    }
                }

                List<string> startLines = new List<string>();

                List<string> endLines = new List<string>();

                for (int l = 0; l < lines.Count; l++)
                {
                    if (l < interfaceLine)
                    {
                        startLines.Add(lines[l]);
                    }
                    else
                    {
                        endLines.Add(lines[l]);
                    }
                }

                int keyId = 0;
                foreach (Type type in itl.DerivedTypes)
                {
                    startLines.Add(GetUnionText(type.FullName, keyId++));
                }

                startLines.AddRange(endLines);


                lines = startLines;

                // Now do the new sourcegen.


                StringBuilder newTxt = new StringBuilder();
                for (int lid = 0; lid < lines.Count; lid++)
                {
                    string line = lines[lid];

                    if (lid == lines.Count - 1 && string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }

                    newTxt.Append(line + "\n");
                }

                File.WriteAllText(fullPath, newTxt.ToString());

            }
        }




        private void RootAddClassAttributes(string codeFolder, List<Type> validTypes)
        {
            string[] allDirectories = Directory.GetDirectories(codeFolder);


            foreach (string directory in allDirectories)
            {
                if (directory.IndexOf(TypePathPrefix) < 0)
                {
                    continue;
                }
                AddClassAttributesInDir(directory, validTypes);
            }
        }

        private void AddClassAttributesInDir(string dirName, List<Type> validTypesLeft)
        {
            string[] fileNames = Directory.GetFiles(dirName);

            foreach (string fileName in fileNames)
            {
                if (fileName.LastIndexOf(".cs") != fileName.Length - 3)
                {
                    continue;
                }

                string fullPath = Path.Combine(dirName, fileName);

                string fileText = File.ReadAllText(fullPath);

                List<string> lines = StrUtils.SplitIntoLines(fileText);

                bool changedSomething = false;

                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l].Contains("public") && lines[l].Contains("class") &&
                        !lines[l].Contains("abstract") && !lines[l].Contains("partial"))
                    {
                        string[] lineWords = lines[l].Split(' ');

                        List<string> validClassWords = new List<string>();

                        foreach (string word in lineWords)
                        {
                            if (!string.IsNullOrEmpty(word) && word.Length > 1)
                            {
                                validClassWords.Add(word.Trim());
                            }
                        }

                        string lastWord = lineWords.Last().Trim();

                        Type finalType = validTypesLeft.FirstOrDefault(x => validClassWords.Contains(x.Name));

                        if (finalType == null)
                        {
                            continue;
                        }

                        lines.Insert(l, StartWhitespace + MpClassAttribute);

                        l += 2;
                        validTypesLeft.Remove(finalType);
                        changedSomething = true;
                        List<PropertyInfo> properties = finalType.GetProperties().ToList();

                        int keyIndex = 0;



                        int curlyBracesCount = 0;
                        while (l < lines.Count && properties.Count > 0)
                        {
                            string trimmedLine = lines[l].Trim();
                            if (trimmedLine.Length < 1)
                            {
                                l++;
                                continue;
                            }

                            if (trimmedLine[0] == '{')
                            {
                                curlyBracesCount++;
                            }

                            else if (trimmedLine[0] == '}')
                            {
                                curlyBracesCount--;
                                if (curlyBracesCount == 0)
                                {
                                    break;
                                }
                            }

                            if (trimmedLine.Contains("public") && trimmedLine.Contains("get") && trimmedLine.Contains("set"))
                            {
                                lines[l] = lines[l].Replace("public ", MpKeyPrefix + keyIndex++ + MpKeySuffix + " public ");
                            }
                            l++;
                        }
                    }
                }


                if (changedSomething)
                {
                    WriteLinesToFile(fullPath, lines);
                }
            }

            string[] directories = Directory.GetDirectories(dirName);

            foreach (string dir in directories)
            {
                AddClassAttributesInDir(dir, validTypesLeft);
            }
        }


        private void RootClearExistingAttributes(string codeFolder)
        {
            string[] allDirectories = Directory.GetDirectories(codeFolder);

            foreach (string directory in allDirectories)
            {
                if (directory.IndexOf(TypePathPrefix) < 0)
                {
                    continue;
                }
                ClearExistingAttributesInDir(directory);
            }
        }

        private void ClearExistingAttributesInDir(string dirName)
        {
            string[] fileNames = Directory.GetFiles(dirName);

            foreach (string fileName in fileNames)
            {
                if (fileName.LastIndexOf(".cs") != fileName.Length - 3)
                {
                    continue;
                }

                if (fileName.IndexOf(typeof(MessagePackInitializer).Name) >= 0)
                {
                    continue;
                }

                string fullPath = Path.Combine(dirName, fileName);

                string fileText = File.ReadAllText(fullPath);

                List<string> startLines = StrUtils.SplitIntoLines(fileText);

                List<string> midLines = new List<string>(startLines);

                bool changedSomething = false;

                List<string> endLines = new List<string>();
                for (int l = 0; l < midLines.Count; l++)
                {
                    bool foundRemovePrefix = false;
                    foreach (LinePrefixPair pair in _attributesToRemove)
                    {
                        if (midLines[l].Contains(pair.Prefix))
                        {
                            if (string.IsNullOrWhiteSpace(pair.Suffix))
                            {
                                foundRemovePrefix = true;
                                changedSomething = true;
                                break;
                            }
                            else
                            {
                                if (midLines[l].IndexOf(MpKeySuffix) >= 0)
                                {
                                    midLines[l] = midLines[l].Substring(0, midLines[l].IndexOf(MpKeyPrefix)) +
                                        midLines[l].Substring(midLines[l].IndexOf(MpKeySuffix) + 3);
                                    changedSomething = true;
                                }
                            }
                        }
                    }

                    if (!foundRemovePrefix)
                    {
                        endLines.Add(midLines[l]);
                    }
                }

                if (changedSomething)
                {
                    WriteLinesToFile(fullPath, endLines);
                }
            }

            string[] subdirs = Directory.GetDirectories(dirName);

            foreach (string subdir in subdirs)
            {
                ClearExistingAttributesInDir(subdir);
            }
        }
        private void WriteLinesToFile(string filePath, List<string> lines)
        {
            StringBuilder sb = new StringBuilder();
            for (int l = 0; l < lines.Count; l++)
            {
                sb.Append(lines[l]);
                if (l < lines.Count - 1)
                {
                    sb.Append('\n');
                }
            }

            string txt = sb.ToString();

            File.WriteAllText(filePath, txt);
        }

    }
}

