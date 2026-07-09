

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

public class ClientBuildVersion : ScriptableObject
{

    public int Version = 1;


#if UNITY_EDITOR
    [MenuItem("Assets/Create/ScriptableObjects/ClientBuildVersion", false, 0)]
    public static void Create()
    {
        ScriptableObjectUtils.CreateBasicInstance<ClientBuildVersion>();
    }

    public static int GetNextBuildVersion()
    {
        ClientBuildVersion settings = ScriptableObjectUtils.LoadDefault<ClientBuildVersion>();

        settings.Version++;

        int newVersion = settings.Version;

        EditorUtility.SetDirty(settings);

        AssetDatabase.SaveAssets();

        return newVersion;
    }
#endif


}


