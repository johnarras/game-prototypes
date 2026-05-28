

using Assets.Scripts.Assets.Constants;
using Assets.Scripts.Audio.ClientEvents;
using Assets.Scripts.CombatFX;
using Assets.Scripts.Core;
using Assets.Scripts.Crawler.ClientEvents.CombatEvents;
using Assets.Scripts.Crawler.Constants;
using OxDb.SharedCore.Utils;
using OxDb.SharedGame.Crawler.Combat.Constants;
using OxDb.SharedGame.Crawler.GameEvents;
using OxDb.SharedGame.Crawler.States.Services;
using OxDb.SharedGame.Factions.Constants;
using OxDb.SharedGame.Spells.Settings.Elements;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.Crawler.Combat
{
    public class CombatEffectUI : BaseBehaviour
    {

        const string CombatHitPrefabSuffix = "CombatHit";

        private ICrawlerService _crawlerService = null;
        protected IClientRandom _rand = null;

        public GImage MainImage;
        public GameObject MainImageParent;
        public GImage HitImage;
        public GameObject DooberTarget;

        public int ScaleAmount = 30;

        public int MaxHitAnchorOffset = 300;

        public float HitImageSizeDelta = 0.5f;


        private int _hitImageFrame = 0;

        private int _attackFrame = AttackFrameCount;
        private const int AttackFrameCount = 4;

        RectTransform _imageTransform;

        private ShowCombatText _nextText;
        private ShowCombatText _currText;
        private int _currFrame = 0;
        private long _factionTypeId = -1;
        Vector2 _startSize = Vector2.zero;
        Vector2 _sizeDelta = Vector2.zero;

        Color _startColor = Color.white;
        Color _targetColor = new Color(1, 0.6f, 0.6f);

        private string _uniqueId = null;

        private CombatHit _currHit;

        private Dictionary<string, CombatHit> _combatHits = new Dictionary<string, CombatHit>();
        public override void Init()
        {
            base.Init();

            _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());

            _dispatcher.AddListener<ShowCombatBolt>(OnShowCombatBolt, GetToken());

            AddUpdate(OnUpdate, UpdateTypes.Regular);

            _clientEntityService.SetActive(HitImage, false);

        }


        public void SetData(string uniqueId, GImage mainImage, GameObject mainImageParent, long factionTypeId)
        {
            _uniqueId = uniqueId;
            MainImage = mainImage;
            MainImageParent = mainImageParent;
            _factionTypeId = factionTypeId;
        }


        public void OnShowCombatText(ShowCombatText text)
        {
            if (text.TargetGroupId != _uniqueId && text.TargetUnitId != _uniqueId)
            {
                return;
            }

            if (text.TextType == ECombatTextTypes.Damage)
            {
                _nextText = text;
            }
        }

        private void OnShowCombatBolt(ShowCombatBolt bolt)
        {

            if (bolt.CasterId == _uniqueId)
            {
                _attackFrame = 0;
            }
        }


        private Vector3 _allyAttackShift = new Vector3(0, 30, 0);
        private Vector3 _enemyAttackShift = new Vector3(0, -30, 0);
        private bool _didInitSizes = false;
        private void OnUpdate()
        {
            if (MainImage == null)
            {
                return;
            }
            if (!_didInitSizes)
            {

                _imageTransform = MainImage.GetComponent<RectTransform>();

                _startSize = _imageTransform.sizeDelta;
                _sizeDelta = new Vector2(ScaleAmount, ScaleAmount);
                _didInitSizes = true;
            }

            if (MainImageParent != null)
            {
                if (_attackFrame < AttackFrameCount)
                {
                    _attackFrame++;

                    float shiftScale = (_attackFrame) * (AttackFrameCount - _attackFrame) * 4.0f / (AttackFrameCount * AttackFrameCount);
                    MainImageParent.transform.localPosition = shiftScale * (_factionTypeId == FactionTypes.Player ? _allyAttackShift : _enemyAttackShift);
                }
                else
                {
                    MainImageParent.transform.localPosition = Vector3.zero;
                }
            }

            if (_currFrame == 0)
            {
                if (_nextText != null)
                {
                    _currText = _nextText;
                    _nextText = null;

                    ElementType elementType = _gameData.Get<ElementTypeSettings>(_gs.ch).Get(_currText.ElementTypeId);
                    if (elementType != null)
                    {
                        _dispatcher.Dispatch(new PlaySound(elementType.Art + "Hit"));

                        if (ColorUtility.TryParseHtmlString(elementType.Color, out Color color))
                        {
                            _targetColor = color;
                        }
                    }

                    string artName = "CombatHit" + _rand.Rand.Next(1, 3);

                    _dispatcher.Dispatch(new PlaySound(CrawlerAudio.MonsterHit));
                    if (_combatHits.ContainsKey(artName))
                    {
                        ShowCombatHitArt(_combatHits[artName]);
                    }
                    else
                    {
                        _assetService.LoadAssetInto(gameObject, AssetCategoryNames.Combat, artName, OnLoadCombatHit, GetToken(), artName);
                    }
                }
            }
            if (_currText == null)
            {
                return;
            }

            int animationFrames = CrawlerCombatConstants.GetScrollingFrames(_crawlerService.GetParty().ScrollFramesIndex);

            if (animationFrames < 2)
            {
                animationFrames = 2;
            }

            int midFrame = animationFrames / 2;

            int frameDelta = Math.Abs(midFrame - _currFrame);

            float midPercent = MathUtil.Clamp(0, 1.0f - frameDelta * 1.0f / midFrame, 1);

            MainImage.color = _startColor * (1 - midPercent) + _targetColor * (midPercent);

            _imageTransform.sizeDelta = _startSize + _sizeDelta * midPercent;

            _currFrame++;
            if (_currFrame >= animationFrames)
            {
                _currFrame = 0;
                _currText = null;

                _clientEntityService.SetActive(HitImage, false);

                MainImage.color = _startColor;
            }

            float hitAlpha = Math.Max(0, midPercent * 2 - 1);

            if (_hitImageFrame >= 0)
            {
                _hitImageFrame++;
                if (_currHit == null || _currHit.Images.Count <= _hitImageFrame / 2)
                {
                    HitImage.SetSingleSprite(null);
                    _clientEntityService.SetActive(HitImage, false);
                    _hitImageFrame = -1;
                }
                else
                {
                    HitImage.SetSingleSprite(_currHit.Images[_hitImageFrame / 2]);
                }
            }
        }

        private void OnLoadCombatHit(GameObject go, string artName, CancellationToken token)
        {
            CombatHit hit = go.GetComponent<CombatHit>();

            if (hit == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            if (_combatHits.ContainsKey(artName))
            {
                _clientEntityService.Destroy(go);
                return;
            }

            _combatHits[artName] = hit;

            _clientEntityService.SetActive(hit.gameObject, false);

            ShowCombatHitArt(hit);
        }

        private void ShowCombatHitArt(CombatHit hit)
        {
            _currHit = null;
            if (HitImage == null || hit.Images.Count < 1)
            {
                return;
            }


            _clientEntityService.SetActive(HitImage, true);

            _currHit = hit;
            HitImage.SetSingleSprite(hit.Images[0]);
            _hitImageFrame = 0;
            RectTransform rectTransform = HitImage.GetComponent<RectTransform>();

            float dx = RandUtils.FloatRange(-MaxHitAnchorOffset, MaxHitAnchorOffset, _rand.Rand);

            float dy = RandUtils.FloatRange(-MaxHitAnchorOffset, MaxHitAnchorOffset, _rand.Rand);

            float angle = 0;

            if (dx < 0)
            {
                if (dy < 0)
                {
                    angle = RandUtils.FloatRange(0, 90, _rand.Rand);
                }
                else
                {
                    angle = RandUtils.FloatRange(-90, 0, _rand.Rand);
                }
            }
            else
            {
                if (dy < 0)
                {
                    angle = RandUtils.FloatRange(90, 180, _rand.Rand);
                }
                else
                {
                    angle = RandUtils.FloatRange(180, 270, _rand.Rand);
                }
            }


            rectTransform.localPosition = new Vector3(dx, dy, 0);

            rectTransform.localEulerAngles = new Vector3(0, 0, angle);

            rectTransform.localScale = Vector3.one * RandUtils.FloatRange(1, 1 + HitImageSizeDelta, _rand.Rand);
        }
    }
}


