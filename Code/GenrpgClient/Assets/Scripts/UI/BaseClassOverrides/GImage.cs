
using Genrpg.Shared.UI.Interfaces;
using UnityEngine;
public class GImage : UnityEngine.UI.Image, IImage
{
    public float FillAmount { get { return fillAmount; } set { fillAmount = value; } }

    public UnityEngine.Color Color { get { return color; } set { color = value; } }

    protected SpriteAtlasContainer _currAtlas = null;

    protected override void OnDestroy()
    {
        SetAtlasSprite(null, null);
        base.OnDestroy();
    }

    public void SetSingleSprite(Sprite spriteArg)
    {
        SetAtlasSprite(null, spriteArg);
    }

    public void SetAtlasSprite(SpriteAtlasContainer atlasContainer, Sprite spriteArg)
    {
        if (_currAtlas != null && (_currAtlas != atlasContainer || spriteArg == null))
        {
            _currAtlas.RemoveRef(this);
        }

        if (atlasContainer != null && atlasContainer != _currAtlas)
        {
            _currAtlas = atlasContainer;
            atlasContainer.AddRef(this);
        }

        sprite = spriteArg;

    }

    public void SetColor(Color colorArg)
    {
        color = colorArg;
    }

    public void SetColor(float r, float g, float b, float a = 1.0f)
    {
        color = new Color(r, g, b, a);
    }
}