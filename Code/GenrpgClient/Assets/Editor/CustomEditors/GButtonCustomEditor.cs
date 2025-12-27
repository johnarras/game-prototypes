using UnityEditor;
using UnityEditor.UI;

namespace Assets.Editor.CustomEditors
{
    [CustomEditor(typeof(GButton))]
    public class GButtonCustomEditor : ButtonEditor
    {

        protected override void OnEnable()
        {
            base.OnEnable(); // Call the base class's OnEnable

        }

    }
}


