using Assets.Scripts.Assets.Bundles;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(BundleList))]
public class BundleListEditor : Editor
{
    private SerializedProperty itemsProp;

    private void OnEnable()
    {
        itemsProp = serializedObject.FindProperty("Bundles");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Bundle Items", EditorStyles.boldLabel);

        for (int i = 0; i < itemsProp.arraySize; i++)
        {
            var itemProp = itemsProp.GetArrayElementAtIndex(i);
            var nameProp = itemProp.FindPropertyRelative("BundleName");
            var localProp = itemProp.FindPropertyRelative("IsLocal");

            EditorGUILayout.BeginHorizontal();
            nameProp.stringValue = EditorGUILayout.TextField(nameProp.stringValue);
            localProp.boolValue = EditorGUILayout.Toggle(localProp.boolValue);
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Add Item"))
        {
            itemsProp.InsertArrayElementAtIndex(itemsProp.arraySize);
            var newItem = itemsProp.GetArrayElementAtIndex(itemsProp.arraySize - 1);
            newItem.FindPropertyRelative("BundleName").stringValue = "New Item";
            newItem.FindPropertyRelative("IsLocal").boolValue = false;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
