using MessagePack;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdFloatCollection : BaseSmallIdQuantityCollection<float>
    {
        public override float[] Data { get; set; } = new float[4];
        protected override float InternalAdd(float first, float second)
        {
            return first + second;
        }

        protected override bool IsDefault(float t)
        {
            return t == 0;
        }
    }
}


