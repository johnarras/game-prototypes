
using UnityEngine;
using UnityEditor;
using Genrpg.Shared.Core.Entities;
using Genrpg.Shared.Constants;

public class ClientBuildVersionSettings : ScriptableObject
{

    public int Version = 0;


    public static string GetVersionAssetPath (string env)
    {
        if (string.IsNullOrEmpty(env))
        {
            env = EnvNames.Test;
        }

        return "Assets/Editor/" + env + "ClientBuildSettings.asset";
    }

    public static void UpdateVersionFile (ClientBuildVersionSettings asset, string env)
    {
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ClientBuildVersionSettings>();
            asset.Version = 1;
            string path = GetVersionAssetPath(env);
            Debug.Log("Create new asset at: " + path);
            AssetDatabase.CreateAsset(asset, path);
        }
        EditorUtility.SetDirty(asset);
        AssetDatabase.SaveAssets();

    }

    public static ClientBuildVersionSettings GetClientVersionFile (string env)
    {
        ClientBuildVersionSettings asset = AssetDatabase.LoadAssetAtPath<ClientBuildVersionSettings>(GetVersionAssetPath(env));
        if (asset == null)
        {
            asset = ScriptableObject.CreateInstance<ClientBuildVersionSettings>();
            AssetDatabase.CreateAsset(asset, GetVersionAssetPath(env));
        }
        return asset;
    }

}
