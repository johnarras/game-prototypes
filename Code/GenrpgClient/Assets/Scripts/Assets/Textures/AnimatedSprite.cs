using OxDb.Client.TextureLists.Services;
using OxDb.SharedCore.Entities.Assets;
using OxDb.SharedCore.Entities.Services;
using OxDb.SharedCore.Utils;
using UnityEngine;

namespace OxDb.Client.Assets.Textures
{
    public class AnimatedSprite : BaseBehaviour
    {

        private ISpriteListCache _spriteListCache;
        protected IEntityService _entityService = null;
        private IClientAppService _clientAppService = null;

        private CachedSpriteList _cachedSpriteList;

        public GImage AnimatedImage;
        public Sprite BlankSprite;

        public bool OnlyShowFirstFrame = false;

        public bool ShowSequence = false;
        public int FramesBetweenSequenceStep = 2;
        public float InitialFrameTimeScale = 1.0f;


        public float MinTimeBetweenRandomFrames = 0.2f;
        public float MaxTimeBetweenRandomFrames = 1.0f;

        private string _currentSpriteName = null;
        private string _newSpriteName = null;

        private int _currentImageFrame = 0;
        private int _ticksBetweenFrameUpdate = 0;

        public override void Init()
        {
            AddUpdate(LateUpdateImage, UpdateTypes.Late);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            ClearCurrentSprite();
        }

        public void SetImage(long entityTypeId, long entityId)
        {
            EntityAtlasIcon args = _entityService.TryGetEntityIcon(_gs.ch, entityTypeId, entityId);

            if (args != null && !string.IsNullOrEmpty(args.IconName))
            {
                SetImage(args.IconName);
            }
            else
            {
                SetImage(null);
            }
        }

        public void SetImage(string spriteName)
        {
            if (spriteName == _currentSpriteName || spriteName == _newSpriteName)
            {
                return;
            }

            _newSpriteName = spriteName;

        }

        private void OnLoadSpriteList(object textureList, object data)
        {
            if (data is DownloadSpriteListArgs downloadData)
            {
                if (_currentSpriteName == downloadData.TextureName)
                {
                    return;
                }
                _currentSpriteName = downloadData.TextureName;

                if (_cachedSpriteList != null)
                {
                    _cachedSpriteList.RemoveRef(this);
                }
                _cachedSpriteList = downloadData.TextureList;
                if (_cachedSpriteList != null)
                {
                    _cachedSpriteList.AddRef(this);
                }
                ShowSpriteFrame(0);
            }
        }

        float _elapsedFrameSeconds = 0;
        float _nextFrameChangeSeconds = 0;
        private void LateUpdateImage()
        {
            string spriteName = _newSpriteName;
            if (_newSpriteName != _currentSpriteName)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    _cachedSpriteList = null;
                    _currentSpriteName = spriteName;
                    ShowSpriteFrame(0);
                    return;
                }
                if (_currentSpriteName == spriteName)
                {
                    return;
                }
                _spriteListCache.LoadSpriteList(spriteName, OnLoadSpriteList, spriteName, GetToken());
                return;
            }

            if (_cachedSpriteList == null || _cachedSpriteList.SpriteList.Sprites.Count < 1)
            {
                ClearCurrentSprite();
                return;
            }

            if (_cachedSpriteList.SpriteList.Sprites.Count == 1)
            {
                return;
            }

            if (!ShowSequence)
            {
                if (!OnlyShowFirstFrame)
                {
                    _elapsedFrameSeconds += _clientAppService.GetDeltaTime();
                    if (_elapsedFrameSeconds >= _nextFrameChangeSeconds)
                    {
                        _elapsedFrameSeconds = 0;
                        _nextFrameChangeSeconds = RandUtils.FloatRange(MinTimeBetweenRandomFrames, MaxTimeBetweenRandomFrames, _gs.Rand);
                        if (_currentImageFrame > 0)
                        {
                            ShowSpriteFrame(0);
                            return;
                        }
                        else
                        {
                            ShowSpriteFrame(RandUtils.IntRange(1, _cachedSpriteList.SpriteList.Sprites.Count - 1, _gs.Rand));
                            return;
                        }
                    }
                }
            }
            else
            {
                if (OnlyShowFirstFrame)
                {
                    if (_currentImageFrame > 0)
                    {
                        ShowSpriteFrame(0);
                    }
                    return;
                }


                int currFrames = FramesBetweenSequenceStep;
                if (_currentImageFrame == 0 && InitialFrameTimeScale > 1)
                {
                    currFrames = (int)(InitialFrameTimeScale * currFrames);
                }
                _ticksBetweenFrameUpdate++;
                if (_ticksBetweenFrameUpdate >= currFrames)
                {
                    _currentImageFrame++;
                    if (_currentImageFrame >= _cachedSpriteList.SpriteList.Sprites.Count)
                    {
                        _currentImageFrame = 0;
                    }
                    ShowSpriteFrame(_currentImageFrame);
                    _ticksBetweenFrameUpdate = 0;
                }
            }

        }


        private void ClearCurrentSprite()
        {
            AnimatedImage.SetSingleSprite(BlankSprite);
            if (_cachedSpriteList != null)
            {
                _cachedSpriteList.RemoveRef(this);
                _cachedSpriteList = null;
            }
            _currentSpriteName = null;
            _newSpriteName = null;
        }

        private void ShowSpriteFrame(int frame)
        {
            if ((_cachedSpriteList == null || _cachedSpriteList.SpriteList == null || _cachedSpriteList.SpriteList.Sprites.Count < 1))
            {
                ClearCurrentSprite();
                return;
            }

            if (_cachedSpriteList.SpriteList.Sprites.Count > frame)
            {
                AnimatedImage.SetSingleSprite(_cachedSpriteList.SpriteList.Sprites[frame]);
            }
            _currentImageFrame = frame;
        }
    }
}


