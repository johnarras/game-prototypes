using Assets.Scripts.Crawler.Combat;
using Assets.Scripts.Crawler.Services.CrawlerMaps;
using Assets.Scripts.TextureLists.Services;
using Genrpg.Shared.Crawler.Maps.Services;
using Genrpg.Shared.Crawler.TextureLists.Services;
using Genrpg.Shared.Utils;
using UnityEngine;

namespace Assets.Scripts.Assets.Textures
{
    public class AnimatedSprite : BaseBehaviour
    {

        private ITextureListCache _textureListCache;
        private ICrawlerMapService _crawlerMapService;

        private SpriteList _textureList;

        public GImage AnimatedImage;
        public Sprite BlankSprite;

        public bool OnlyShowFirstFrame = false;


        public bool ShowSequence = false;
        public int FramesBetweenSequenceStep = 2;

        const float ChangeToBaseFrameChance = 0.2f;
        const float ChangeToOtherFrameChance = 0.05f;

        private string _currentSpriteName = null;
        private string _newSpriteName = null;
        private string _downloadingSpriteName = null;

        private int _currentImageFrame = 0;
        private int _ticksBetweenFrameUpdate = 0;

        public override void Init()
        {
            _updateService.AddUpdate(this, LateUpdatePicture, UpdateTypes.Late, GetToken());
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
                _textureList = downloadData.TextureList;
                ShowTextureFrame(0);
                _downloadingSpriteName = null;
            }
        }


        

        private void LateUpdatePicture()
        {

            string spriteName = _newSpriteName;
            if (_newSpriteName != _currentSpriteName)
            {
                if (string.IsNullOrEmpty(spriteName))
                {
                    _textureList = null;
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

            if (_textureList == null || _textureList.Sprites.Count < 1)
            {
                _uiService.SetImageSprite(AnimatedImage, BlankSprite);
                return;
            }

            if (_textureList.Sprites.Count == 1)
            {
                return;
            }

            if (!ShowSequence)
            {
                if (!OnlyShowFirstFrame)
                {
                    if (_currentImageFrame > 0 && _rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(0);
                        return;
                    }

                    if (_currentImageFrame == 0 && _rand.NextDouble() < ChangeToBaseFrameChance)
                    {
                        ShowTextureFrame(MathUtils.IntRange(1, _textureList.Sprites.Count - 1, _rand));
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

                _ticksBetweenFrameUpdate++;
                if (_ticksBetweenFrameUpdate >= FramesBetweenSequenceStep)
                {
                    _currentImageFrame++;
                    if (_currentImageFrame >= _textureList.Sprites.Count)
                    {
                        _currentImageFrame = 0;
                    }
                    ShowTextureFrame(_currentImageFrame);
                    _ticksBetweenFrameUpdate = 0;
                }
            }

        }


        private void ShowTextureFrame(int frame)
        {
            if ((_textureList == null || _textureList.Sprites.Count < 1))
            {
                _uiService.SetImageSprite(AnimatedImage, BlankSprite);
                return;
            }

            if (_textureList.Sprites.Count > frame)
            {
                _uiService.SetImageSprite(AnimatedImage, _textureList.Sprites[frame]);
            }
            _currentImageFrame = frame;
        }
    }
}
