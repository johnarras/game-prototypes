using Assets.Scripts.ProcGen.Materials.Constants;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class FlatPlaneMaterialGenHelper : BaseMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.FlatPlane;

        public override async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            Texture2D tex = new Texture2D(state.Size, state.Size, TextureFormat.RGBAFloat, false);
            state.Block = new MaterialGenBlock(state.Size, state.ForegroundMain, MaterialGenConstants.DefaultStartBrightness, MaterialGenConstants.DefaultStartBumpHeight);


            state.Settings.MaxColorNoiseBumpScale = 0.005f;
            _materialGenUtilsService.AddColorNoise(state);

            _materialGenUtilsService.SmoothColors(state);

            _materialGenUtilsService.ApplyBlockToTexture(state, state.Block, tex);

            return tex;
        }
    }
}
