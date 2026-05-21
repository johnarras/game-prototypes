using OxDb.SharedCore.Client.Contants;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.Build;

public class PlatformBuildData
{
    public BuildTarget Target;
    public NamedBuildTarget NamedTarget;
    public string FilePath;
    public string ClientPlatform;
    public string ApplicationSuffix;

    public string GetBundleOutputPath()
    {

        return BuildConfiguration.AssetBundleRoot + FilePath;
    }

    public string GetTextFileOutputPath()
    {
        Assembly assemb = Assembly.GetExecutingAssembly();
        string loc = assemb.Location;
        return Path.GetDirectoryName(loc) + "/../../" + GetBundleOutputPath();
    }
}

public class BuildConfiguration
{
    public const string AssetBundleRoot = "AssetBundles/";


    public static List<PlatformBuildData> GetbuildConfigs()
    {
        List<PlatformBuildData> list = new List<PlatformBuildData>();

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneWindows,
            NamedTarget = NamedBuildTarget.Standalone,
            FilePath = ClientPlatformNames.Win,
            ClientPlatform = ClientPlatformNames.Win,
            ApplicationSuffix = ".exe",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.Android,
            NamedTarget = NamedBuildTarget.Android,
            FilePath = ClientPlatformNames.Android,
            ClientPlatform = ClientPlatformNames.Android,
            ApplicationSuffix = ".apk",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.iOS,
            NamedTarget = NamedBuildTarget.iOS,
            FilePath = ClientPlatformNames.iOS,
            ClientPlatform = ClientPlatformNames.iOS,
            ApplicationSuffix = ".app",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneLinux64,
            NamedTarget = NamedBuildTarget.Standalone,
            FilePath = ClientPlatformNames.Linux,
            ClientPlatform = ClientPlatformNames.Linux,
            ApplicationSuffix = ".app",
        });

        return list;
    }
}


