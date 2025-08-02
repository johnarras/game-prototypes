using System.Reflection;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using Scripts.Assets.Assets.Constants;
using Genrpg.Shared.Client.Core;

public class PlatformBuildData
{
    public BuildTarget Target;
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


    public static List<PlatformBuildData> GetbuildConfigs(IClientGameState gs)
    {
        List<PlatformBuildData> list = new List<PlatformBuildData>();

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneWindows,
            FilePath = PlatformAssetPrefixes.Win,
            ClientPlatform = ClientPlatformNames.Win,
            ApplicationSuffix = ".exe",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.Android,
            FilePath = PlatformAssetPrefixes.Android,
            ClientPlatform = ClientPlatformNames.Android,
            ApplicationSuffix = ".apk",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.iOS,
            FilePath = PlatformAssetPrefixes.IOS,
            ClientPlatform = ClientPlatformNames.iOS,
            ApplicationSuffix = ".app",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneOSX,
            FilePath = PlatformAssetPrefixes.OSX,
            ClientPlatform = ClientPlatformNames.OSX,
            ApplicationSuffix = ".app",
        });

        list.Add(new PlatformBuildData()
        {
            Target = BuildTarget.StandaloneLinux64,
            FilePath = PlatformAssetPrefixes.Linux,
            ClientPlatform = ClientPlatformNames.Linux,
            ApplicationSuffix = ".app",
        });

        return list;
    }
}
