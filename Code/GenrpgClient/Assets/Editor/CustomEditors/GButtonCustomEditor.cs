using UnityEditor;
using UnityEditor.UI;

namespace Assets.Editor.CustomEditors
{
    [CustomEditor(typeof(GButton))]
    public class GButtonCustomEditor : ButtonEditor
    {

        private SerializedProperty _tooltip;
        protected override void OnEnable()
        {
            base.OnEnable(); // Call the base class's OnEnable
                             // Find the serialized property using its exact variable name.
            _tooltip = serializedObject.FindProperty("Tooltip");
        }

        public override void OnInspectorGUI()
        {
            // First, draw the default inspector for the Button
            base.OnInspectorGUI();

            // This ensures all pending modifications are applied
            serializedObject.ApplyModifiedProperties();

            // Draw our custom field
            EditorGUILayout.PropertyField(_tooltip);

            // Apply any changes made in the inspector
            serializedObject.ApplyModifiedProperties();
        }
    }
}
