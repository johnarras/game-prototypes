

using Assets.Scripts.WorldCanvas.GameEvents;
using Genrpg.Shared.Crawler.Combat.Constants;
using Genrpg.Shared.Crawler.GameEvents;
using Genrpg.Shared.Utils;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

namespace Assets.Scripts.UI.CombatTexts
{

    public class FastCombatTextArgs
    {
        public float AnimateTime { get; set; }
        public float FrameDx { get; set; } = 0;
        public float FrameDy { get; set; } = 1;
        public string Text { get; set; }
        public Color Color { get; set; } = Color.white;
    }


    public class FastCombatTextUI : BaseBehaviour
    {

        private IClientAppService _appService;

        public float AnimateTime = 1.0f;
        public float TextMoveSpeed = 30.0f;

        private const string _subDirectory = "CrawlerCombat";
        private const string _prefabName = "BaseCombatText";

        private ConcurrentQueue<ShowCombatText> _textsToShow = new ConcurrentQueue<ShowCombatText>();

        private List<FastCombatText> _texts = new List<FastCombatText>();

        private long _framesPerSecond = 30;

        private string _unitId;
        public void SetUnitId(string unitId)
        {
            _unitId = unitId;
        }

        private string _groupId;
        public void SetGroupId(string groupId)
        {
            _groupId = groupId;
        }

        public override void Init()
        {
            base.Init();

            _framesPerSecond = _appService.TargetFrameRate;
            _dispatcher.AddListener<ShowCombatText>(OnShowCombatText, GetToken());

            _updateService.AddUpdate(this, LoadNewTexts, UpdateTypes.Regular, GetToken());
        }

        private void OnShowCombatText(ShowCombatText showCombatText)
        {
            if (!string.IsNullOrEmpty(_groupId))
            {
                if (_groupId != showCombatText.TargetGroupId)
                {
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(_unitId) || showCombatText.TargetUnitId != _unitId)
                {
                    return;
                }
            }

            _textsToShow.Enqueue(showCombatText);
        }

        private void LoadNewTexts()
        {

            while (_textsToShow.TryDequeue(out ShowCombatText showCombatText))
            {

                Color textColor = Color.white;

                if (showCombatText.TextType == ECombatTextTypes.Damage)
                {
                    textColor = Color.red;
                }
                else if (showCombatText.TextType == ECombatTextTypes.Healing)
                {
                    textColor = Color.green;
                }
                else if (showCombatText.TextType == ECombatTextTypes.Defense)
                {
                    textColor = new Color(1, 0.75f, 0, 1);
                }
                else if (showCombatText.TextType == ECombatTextTypes.Thorns)
                {
                    textColor = new Color(0.66f, 0, 0.8f, 1);
                }

                float angle = MathUtils.FloatRange(-45, 225, _rand);
                float frameDy = Mathf.Sin(angle * Mathf.PI / 180) * TextMoveSpeed / _framesPerSecond;
                float frameDx = Mathf.Cos(angle * Mathf.PI / 180) * TextMoveSpeed / _framesPerSecond;
                float startFrames = MathUtils.FloatRange(0, 20, _rand);

                Vector3 startPos = transform.position +
                    new Vector3(frameDx * startFrames, frameDy * startFrames, 0);

                FastCombatTextArgs args = new FastCombatTextArgs()
                {
                    AnimateTime = AnimateTime,
                    Color = textColor,
                    Text = showCombatText.Text,
                    FrameDx = frameDx,
                    FrameDy = frameDy,
                };


                ShowDynamicUIItem showUIItem = new ShowDynamicUIItem
                (DynamicUILocation.ScreenSpace,
                _prefabName,
                startPos,
                OnLoadFrameText,
                args,
                GetToken(),
                _subDirectory);

                _dispatcher.Dispatch(showUIItem);
            }
        }

        private void OnLoadFrameText(object obj, object data, CancellationToken token)
        {
            GameObject go = obj as GameObject;

            if (go == null)
            {
                return;
            }

            FastCombatTextArgs args = data as FastCombatTextArgs;

            if (args == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            FastCombatText fastText = _clientEntityService.GetComponent<FastCombatText>(go);
            if (fastText == null)
            {
                _clientEntityService.Destroy(go);
                return;
            }

            fastText.Init(args);
        }
    }
}
