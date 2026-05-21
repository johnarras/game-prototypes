using Assets.Scripts.ProcGen.Materials.Constants;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class FlatPlaneMaterialGenHelper : BaseMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.FlatPlane;

        public override async Awaitable<Texture2D> GenerateTexture(MaterialGenState state)
        {
            Texture2D tex = CreateTexture(state.Width, state.Height);
            state.Block = new MaterialGenBlock(state.Width, state.Height, state.ForegroundMain, MaterialGenConstants.DefaultStartBrightness, MaterialGenConstants.DefaultStartBumpHeight);


            state.Settings.MaxColorNoiseBumpScale = 0.005f;
            state.Settings.MinColorNoiseAmp *= 2;
            state.Settings.MaxColorNoiseAmp *= 2;

            _materialGenUtilsService.AddColorNoise(state);

            _materialGenUtilsService.SmoothColors(state);

            _materialGenUtilsService.ApplyBlockToTexture(state, state.Block, tex);

            await Task.CompletedTask;
            return tex;
        }
    }
}
