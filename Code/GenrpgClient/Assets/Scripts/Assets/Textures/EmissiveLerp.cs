using OxDb.Client.Assets.Materials;
using UnityEngine;

namespace OxDb.Client.Assets.Textures
{
    public class EmissiveLerp : BaseBehaviour
    {

        public MeshRenderer Renderer;
        public float PulseTime = 1.5f;
        public Color StartColor = Color.black;
        public Color EndColor = Color.gold;

        private Material _mat = null;

        private IClientAppService _appService = null;
        public override void Init()
        {

            if (Renderer == null || PulseTime < 0.1f)
            {
                return;
            }

            _mat = Renderer.material;

            if (_mat == null)
            {
                return;
            }

            _mat.EnableKeyword(MaterialUtils.EmissionColorPropertyName);

            _updateService.AddUpdate(this, FrameUpdate, UpdateTypes.Regular, GetToken());

        }

        public void SetColor(Color c)
        {
            c = c * 1.1f;
            EndColor = c;
            if (_mat == null)
            {
                return;
            }
            _mat.SetColor(MaterialUtils.BaseColorPropertyName, c);
        }

        private float _elapsedTime = 0;
        private void FrameUpdate()
        {
            if (PulseTime <= 0)
            {
                return;
            }

            _elapsedTime += _appService.GetDeltaTime();

            while (_elapsedTime > PulseTime)
            {
                _elapsedTime -= PulseTime;
            }

            float midPct = 0;
            if (_elapsedTime < PulseTime / 2)
            {
                midPct = Mathf.SmoothStep(0, 1, _elapsedTime / (PulseTime / 2));
            }
            else
            {
                midPct = 1 - Mathf.SmoothStep(0, 1, (_elapsedTime - PulseTime / 2) / (PulseTime / 2));
            }

            Color c = midPct * StartColor + (1 - midPct) * EndColor;

            _mat.SetColor(MaterialUtils.EmissionColorPropertyName, c);

        }
    }
}
