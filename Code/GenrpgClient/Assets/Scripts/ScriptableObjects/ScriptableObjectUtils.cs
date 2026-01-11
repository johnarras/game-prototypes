#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine; // Needed

public class ScriptableObjectUtils
{
    const string _baseConfigPath = "Assets/Resources/" + ConfigResourcesFolder;
    public const string ConfigResourcesFolder = "Config/";

#if UNITY_EDITOR
    public static void CreateBasicInstance<T>() where T : ScriptableObject
    {

        string classname = typeof(T).Name;
        string fullPath = _baseConfigPath + classname + ".asset";
        ScriptableObject so = ScriptableObject.CreateInstance(typeof(T));
        AssetDatabase.CreateAsset(so, fullPath);
        AssetDatabase.Refresh();
    }
#endif

    public static T LoadDefault<T>() where T : ScriptableObject
    {
        T obj = Resources.Load<T>(ConfigResourcesFolder + typeof(T).Name);
        return obj;
    }
}


