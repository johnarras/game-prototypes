using System;
using UnityEngine;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Core.Constants;
using Genrpg.Shared.Config.Constants;






#if UNITY_EDITOR
using UnityEditor;
#endif
using Genrpg.Shared.Constants;


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
    public bool SelfContainedClient;
    public bool ExportGameData;

    public string GooglePlaySecret;
    public string IOSSecret;
    public string PackageName;

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

