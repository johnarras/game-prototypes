using UnityEngine.UI;


public class GToggle : BaseBehaviour
{

    public GText Text;
    public Toggle Toggle;

    public void Init(string text, bool isSelected)
    {
        Toggle.isOn = isSelected;
        _uiService.SetText(Text, text);
    }

    public bool IsOn()
    {
        return Toggle.isOn;
    }
}
