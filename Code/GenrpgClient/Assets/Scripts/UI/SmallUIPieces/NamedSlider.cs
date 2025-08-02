using Assets.Scripts.UI.Abstractions;
using System;

namespace Assets.Scripts.UI.Core
{
    public class NamedSlider : BaseBehaviour
    {
        public GText Name;
        public GSlider Slider;

        private Action<float> _valueChangedEvent;
        public void InitSlider(float minValue, float maxValue, float currValue, bool wholeNumbers, Action<float> valueChangedEvent)
        {
            Slider.Init(minValue, maxValue, currValue, valueChangedEvent);
            Slider.wholeNumbers = wholeNumbers;
        }
    }
}
