using Assets.Scripts.ProcGen.Materials.Constants;

namespace Assets.Scripts.ProcGen.Materials.MaterialGenHelpers
{
    public class LargeBlockMaterialGenHelper : BlocksMaterialGenHelper
    {
        public override EMaterialGenTypes HelperKey => EMaterialGenTypes.LargeBlocks;


        protected override void TweakStateValues(MaterialGenState state)
        {
            state.BlockRowCount = state.Rand.Next(3, 5);
            state.CornerPerturbChance = 1;
            state.MaxCornerPerturbScale = 0.5f;
            state.VerticalPerturbChance = 0;
        }
    }
}
