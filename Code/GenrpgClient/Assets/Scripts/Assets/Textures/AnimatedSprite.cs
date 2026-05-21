using Assets.Scripts.Core;
using Assets.Scripts.TextureLists.Services;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.TextureLists.Services;
using UnityEngine;

namespace Assets.Scripts.Assets.Textures
{
    public class AnimatedSprite : BaseBehaviour
    {

        private ITextureListCache _textureListCache;
        protected IClientRandom _rand = null;

        private CachedSpriteList _cachedSpriteList;

        public GImage AnimatedImage;
        public Sprite BlankSprite;

        public bool OnlyShowFirstFrame = false;

        public bool ShowSequence = false;
        public int FramesBetweenSequenceStep = 2;
        public float InitialFrameTimeScale = 1.0f;

        const float ChangeToBaseFrameChance = 0.2f;

        private string _currentSpriteName = null;
        private string _newSpriteName = null;
        private string _downloadingSpriteName = null;

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

        public void SetImage(string spriteName)
        {
            if (spriteName == _currentSpriteName || spriteName == _newSpriteName)
            {
                return;
            }

            _newSpriteName = spriteName;

        }

        private void OnLoadTextureList(object textureList, object data)
        {
            if (data is DownloadTextureListData downloadData)
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
                ShowTextureFrame(0);
                _downloadingSpriteName = null;
            }
        }

        private void LateUpdateImage()
        {
            string spriteName = _newSpriteName;
            if (_newSpriteName != _currentSpriteName)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    _cachedSpriteList = null;
                    _currentSpriteName = spriteName;
                    ShowTextureFrame(0);
                    return;
                }
                if (_currentSpriteName == spriteName)
                {
                    return;
                }
                _downloadingSpriteName = spriteName;
                _textureListCache.LoadTextureList(spriteName, OnLoadTextureList, spriteName, GetToken());
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
                    if (_currentImageFrame > 0 && _rand.Rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(0);
                        return;
                    }

                    if (_currentImageFrame == 0 && _rand.Rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(RandUtils.IntRange(1, _cachedSpriteList.SpriteList.Sprites.Count - 1, _rand.Rand));
                        return;
                    }
                }
            }
            else
            {
                if (OnlyShowFirstFrame)
                {
                    if (_currentImageFrame > 0)
                    {
                        ShowTextureFrame(0);
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
                    ShowTextureFrame(_currentImageFrame);
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

        private void ShowTextureFrame(int frame)
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


