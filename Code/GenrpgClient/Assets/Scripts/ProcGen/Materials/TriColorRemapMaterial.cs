
using Assets.Scripts.Assets.Materials;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials
{
    public class TriColorRemapMaterial : BaseBehaviour
    {

        private IModTextureService _modTextureService = null;
        public Renderer Renderer;


        public float EmissionScale = 1.0f;
        public Color EmissionColor;

        public void SetColors(Color redColor, Color greenColor, Color blueColor)
        {
            MaterialPropertyBlock colorBlock = new MaterialPropertyBlock();
            colorBlock.SetColor("_RedRemap", redColor);
            colorBlock.SetColor("_GreenRemap", greenColor);
            colorBlock.SetColor("_BlueRemap", blueColor);

            if (EmissionScale > 1)
            {
                Color emissionColor = _modTextureService.GetNeonColor(redColor, EmissionScale);
                colorBlock.SetColor(MaterialUtils.EmissionColorPropertyName, emissionColor);
            }
            else
            {
                colorBlock.SetColor(MaterialUtils.EmissionColorPropertyName, Color.black);
            }
            Renderer.SetPropertyBlock(colorBlock);
        }

        public void SetColors(string redHex, string greenHex = "#00FF00", string blueHex = "#0000FF")
        {
            Color redColor = _modTextureService.ParseHtmlHexColor(redHex);
            Color greenColor = _modTextureService.ParseHtmlHexColor(greenHex);
            Color blueColor = _modTextureService.ParseHtmlHexColor(blueHex);

            SetColors(redColor, greenColor, blueColor);
        }
    }
}
