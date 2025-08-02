using System;
using UnityEngine;
using System.Collections.Generic;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Constants;





#if UNITY_EDITOR
using UnityEditor;
#endif
using Genrpg.Shared.Constants;


public interface IClientConfigContainer : IInjectable, IExplicitInject
{
   ClientConfig Config { get; set; }
}

public class ClientConfigContainer : IClientConfigContainer
{
    public ClientConfig Config { get; set; }
}

[Serializable]
public class ClientConfig : ScriptableObject
{
    public EGameModes GameMode = EGameModes.Crawler;
    public string Env = EnvNames.Dev;
    public string WebEndpointOverride;
    public string AssetEnvOverride;
    public string WorldEnvOverride;
    public int AccountProductId = 2;
    public bool SelfContainedClient;
    public bool ExportGameData;

    public string GooglePlaySecret;
    public string IOSSecret;
    public string PackageName;

    public string GetWebEndpoint ()
    {
        if (Env == EnvNames.Local)
        {
            return "http://localhost:5000";
        }
        return "https://" + Env.ToLower() + "-genrpg.azurewebsites.net";
    }

    public string GetContentRoot()
    {
        return AssetConstants.DefaultDevContentRoot;
    }

    public string GetAssetDataEnv()
    {
        return GetDefaultOrOverride(AssetEnvOverride);
    }
    public string GetWorldDataEnv()
    {
        return GetDefaultOrOverride(WorldEnvOverride);
    }

    private string GetDefaultOrOverride(string overrideEnv)
    {
        string assetEnv = overrideEnv;

        if (string.IsNullOrEmpty(assetEnv))
        {
            assetEnv = Env;
        }
        if (assetEnv == EnvNames.Local)
        {
            assetEnv = EnvNames.Dev;
        }
        return assetEnv;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientConfig", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientConfig>();
    }
#endif

    public string GetConnectionString(string key)
    {
        return "";
    }

    public Dictionary<string, string> GetConnectionStrings()
    {
        return new Dictionary<string, string>();
    }
}