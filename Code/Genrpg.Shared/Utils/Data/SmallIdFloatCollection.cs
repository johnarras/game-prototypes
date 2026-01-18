using MessagePack;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdFloatCollection : BaseSmallIdQuantityCollection<float>
    {
        public float[] Data { get => _data; set => _data = value; }
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


