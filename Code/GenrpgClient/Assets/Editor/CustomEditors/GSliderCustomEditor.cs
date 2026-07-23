using OxDb.Client.UI.Abstractions;
using UnityEditor;
using UnityEditor.UI;

namespace Assets.Editor.CustomEditors
{
    [CustomEditor(typeof(GSlider))]
    public class GSliderCustomEditor : SliderEditor
    {

        protected override void OnEnable()
        {
            base.OnEnable(); // Call the base class's OnEnable

        }

    }
}


