

using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Assets.Sprites.Services;
using UnityEngine;

public class MoneySegment : BaseBehaviour
{

    protected ISpriteService _spriteService = null;

    public GameObject Parent;
    public GText QuantityText;
    public GImage Icon;
    public string IconName;
    public GameObject GetParent()
    {
        return Parent;
    }

    public override void Init()
    {
        base.Init();
        if (!string.IsNullOrEmpty(_txt))
        {
            SetQuantityText(_txt);
        }
        if (!string.IsNullOrEmpty(IconName))
        {
            _spriteService.SetAtlasSpriteInto(AtlasNames.UI, IconName, Icon, GetToken());
        }
    }

    private string _txt = null;
    public void SetQuantityText(string txt)
    {
        _txt = txt;
        _uiService?.SetText(QuantityText, txt);
    }
}


