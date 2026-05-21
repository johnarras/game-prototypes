using OxDb.SharedCore.Config.Constants;
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
    public class SystemTextJsonInitializer
    {
        readonly List<LinePrefixPair> _attributesToRemove = new List<LinePrefixPair>()
        {
           new LinePrefixPair() { Prefix = JsonDerivedPrefix },
           new LinePrefixPair() { Prefix = JsonPolymorphic },
           new LinePrefixPair() { Prefix = JsonSourceGenOptions },
        };

        const string JsonDerivedPrefix = "[JsonDerivedType(typeof(";
        const string JsonDerivedMiddle = "),nameof(";
        const string JsonDerivedSuffix = "))]";
        const string JsonPolymorphic = "[JsonPolymorphic]";
        const string JsonSourceGenOptions = "[JsonSourceGenerationOptions";

        const string JsonSourceGenContext = "JsonGenerationContext";

        const string JsonSourceGenRegion = "SourceGen";

        const string StartWhitespace = "    ";

        const string TypePathPrefix = "OxDb.";

        readonly List<string> _neededUsings = new List<string>()
        {
            "System",
            "System.Collections.Generic",
            "System.Text.Json",
            "System.Text.Json.Serialization",
            "System.Text.Json.Serialization.Metadata",
        };


        readonly List<string> _sourceGenLines = new List<string>()
        {
            "#region SourceGen",
            "[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]",
            "[JsonSerializable(typeof(XXXXX))]",
            "[JsonSerializable(typeof(List<XXXXX>))]",
            "public partial class XXXXXJsonGenerationContext : JsonSerializerContext",
            "{",
            //"    public XXXXXJsonGenerationContext(JsonSerializerOptions? options) : base(options) { }",
            //"",
            //"    protected override JsonSerializerOptions GeneratedSerializerOptions => JsonSerializerOptions.Default;",
            //"",
            //"    public override JsonTypeInfo GetTypeInfo(Type type)",
            //"    {",
            //"        if (type == typeof(XXXXX))",
            //"        {",
            //"            return JsonTypeInfo.CreateJsonTypeInfo<XXXXX>(GeneratedSerializerOptions);",
            //"        }",
            //"",
            //"        return null;",
            //"    }",
            "}",
            "#endregion SourceGen",
        };


        private IReflectionService _reflectionService = null;
        public void Init(string dirName, IReflectionService reflectionService, Assembly topLevelAssembly)
        {
            _reflectionService = reflectionService;

            RootClearExistingAttributes(dirName);
            List<Type> allTypes = GetAllTypes(topLevelAssembly);
            List<Type> validInterfaces = GetValidInterfaces(allTypes);

            List<InterfaceTypeList> typeList = GetInterfaceTypeLists(validInterfaces, allTypes);

            List<Type> validClasses = GetValidClasses(typeList, allTypes);

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

            return retval;
        }


        private bool IsValidType(Type t)
        {

            if (!_reflectionService.IsValidReflectionType(t) || t.IsInterface ||
                string.IsNullOrEmpty(t.FullName) ||
                t.FullName.IndexOf("Genrpg.Editor") >= 0 ||
                t.FullName.IndexOf(TypePathPrefix) != 0)
            {
                return false;
            }

            SystemTextJsonIgnoreTypeAttribute? attr = t.GetCustomAttribute<SystemTextJsonIgnoreTypeAttribute>(true);

            return attr == null;
        }

        private List<Type> GetValidInterfaces(List<Type> allTypes)
        {

            List<Type> interfacesToSetup = new List<Type>();

            foreach (Type type in allTypes)
            {
                SystemTextJsonInterfaceAttribute? interfaceProp = type.GetCustomAttribute<SystemTextJsonInterfaceAttribute>(true);

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


                foreach (string usingType in _neededUsings)
                {
                    string usingLine = "using " + usingType + ";";

                    if (!lines.Any(x => x.Contains(usingLine)))
                    {
                        lines.Insert(0, usingLine);
                    }
                }


                string lookFor = "public interface " + itype.Name;

                int interfaceLine = -1;

                List<string> lines2 = new List<string>();

                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l].Contains(JsonPolymorphic) ||
                        lines[l].Contains(JsonDerivedPrefix))
                    {
                        continue;
                    }
                    lines2.Add(lines[l]);
                }

                lines = lines2;

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

                startLines.Add(StartWhitespace + JsonPolymorphic);

                foreach (Type type in itl.DerivedTypes)
                {
                    startLines.Add(StartWhitespace + JsonDerivedPrefix + type.FullName + JsonDerivedMiddle + type.FullName + JsonDerivedSuffix);
                }

                // Now do the sourcegen context.

                for (int l = 0; l < lines.Count; l++)
                {
                    if (lines[l].Contains(itype.Name + JsonSourceGenContext))
                    {
                        lines.RemoveAt(l);
                        break;
                    }
                }

                lines = startLines;

                lines.AddRange(endLines);

                // Now do the new sourcegen.

                int lastCurlyBraceLine = -1;

                for (int currLine = lines.Count - 1; currLine >= 0; currLine--)
                {
                    if (lines[currLine].IndexOf('}') == 0)
                    {
                        lastCurlyBraceLine = currLine;
                        break;
                    }
                }

                // Now add the block of text for the source generator.

                for (int l = 0; l < _sourceGenLines.Count; l++)
                {
                    // Cannot use this yet.
                    lines.Insert(lastCurlyBraceLine + l, StartWhitespace + _sourceGenLines[l].Replace(AppConfigKeys.PlaceholderString, itype.Name));
                }


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

                if (fileName.IndexOf(typeof(SystemTextJsonInitializer).Name) >= 0)
                {
                    continue;
                }

                string fullPath = Path.Combine(dirName, fileName);

                string fileText = File.ReadAllText(fullPath);

                List<string> startLines = StrUtils.SplitIntoLines(fileText);

                List<string> midLines = new List<string>(startLines);

                int regionStart = -1;
                int regionEnd = -1;

                bool changedSomething = false;
                for (int l = 0; l < midLines.Count; l++)
                {
                    if (midLines[l].Contains("#region " + JsonSourceGenRegion))
                    {
                        regionStart = l;
                    }
                    if (midLines[l].Contains("#endregion " + JsonSourceGenRegion))
                    {
                        regionEnd = l;
                    }
                }

                if (regionStart >= 0 && regionEnd >= 0 && regionEnd > regionStart)
                {
                    int linesToRemove = regionEnd - regionStart + 1;

                    for (int i = 0; i < linesToRemove; i++)
                    {
                        midLines.RemoveAt(regionStart);
                        changedSomething = true;
                    }
                }
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

