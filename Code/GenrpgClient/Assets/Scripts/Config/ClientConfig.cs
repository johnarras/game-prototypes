using System;
using UnityEngine;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Core.Constants;
using OxDb.SharedCore.Config.Constants;
using OxDb.SharedCore.Environments.Constants;

#if UNITY_EDITOR
using UnityEditor;
#endif


public interface IClientConfigContainer : IInjectable, IExplicitInject
{
    ClientConfig Config { get; }
}

public class ClientConfigContainer : IClientConfigContainer
{
    public ClientConfig Config { get; }

    public ClientConfigContainer(ClientConfig config)
    {
        Config = config;
    }
}



[Flags]
public enum ClientPlayerFlags
{
    None = 0,
    SelfContainedClient = 1 << 0,
    ExportGameData = 1 << 1,
    EncryptGameData = 1 << 2,
    VerboseLogging = 1 << 3,
    DevelopmentBuild = 1 << 4,
    BuildAppBundle = 1 << 5,
    IsCouldBuild = 1 << 6,
    SideloadBuild = 1 << 7,
    AllowDebugging = 1 << 8,
    IsDemo = 1 << 9,
}



[Serializable]
public class ClientConfig : ScriptableObject
{
    public EGameModes GameMode = EGameModes.Crawler;
    public string Env = EnvNames.Dev;
    public string BaseWebEndpoint;
    public string ContentEndpoint;
    public string AssetsEnv;
    public string WorldsEnv;
    public int ProductId = 2;

    [field: SerializeField]
    public ClientPlayerFlags Flags = ClientPlayerFlags.None;

    public string LogalyticsConnectionString;
    public string GooglePlayAndroidClientId;
    public string IOSSecret;
    public string PackageName;
    public string AndroidAdsGameKey;
    public string IOSAdsGameKey;

    public string GetWebEndpoint()
    {
        if (Env == EnvNames.Local)
        {
            return "http://localhost:5000";
        }
        return BaseWebEndpoint.Replace(AppConfigKeys.PlaceholderString, Env.ToLower());
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientConfig", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientConfig>();
    }
#endif
}

