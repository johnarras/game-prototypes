
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.U2D;

public class SpriteAtlasContainer : BaseBehaviour
{

    public float TtlSeconds = AssetConstants.DefaultTtl;
    public SpriteAtlas Atlas;

    private Dictionary<string, Sprite> _spriteDict = null;


    protected DateTime _unloadTime = DateTime.UtcNow.AddSeconds(AssetConstants.DefaultTtl);

    public Sprite GetSprite(string spriteName)
    {
        UpdateUnloadTime();
        if (_spriteDict == null)
        {
            Sprite[] _sprites = new Sprite[Atlas.spriteCount];

            Atlas.GetSprites(_sprites);

            _spriteDict = new Dictionary<string, Sprite>();

            for (int s = 0; s < _sprites.Length; s++)
            {
                _spriteDict[_sprites[s].name.Replace("(Clone)", "")] = _sprites[s];
            }
        }

        if (_spriteDict.TryGetValue(spriteName, out Sprite spr))
        {
            return spr;
        }
        return null;

    }

    private List<GImage> _refs = new List<GImage>();

    public bool CanUnload()
    {
        return _unloadTime <= DateTime.UtcNow && !_refs.Any(x => x != null);
    }

    public void AddRef(GImage image)
    {
        if (image == null || _refs.Contains(image))
        {
            return;
        }
        _refs.Add(image);
        UpdateUnloadTime();
    }

    public void RemoveRef(GImage image)
    {
        _refs.Remove(image);
        UpdateUnloadTime();
    }

    public void UpdateUnloadTime()
    {
        _unloadTime = DateTime.UtcNow.AddSeconds(TtlSeconds);
    }

}