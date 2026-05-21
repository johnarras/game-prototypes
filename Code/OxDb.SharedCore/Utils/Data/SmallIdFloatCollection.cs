using System.Linq;

namespace OxDb.SharedCore.Utils.Data
{
    public class SmallIdFloatCollection : BaseSmallIdQuantityCollection<float>
    {
        public float[] Data { get => _data; set => _data = value; }
        protected override float InternalAdd(float first, float second)
        {
            return first + second;
        }

        public override long GetAccumulation() { return (long)_data.Sum(); }
        protected override bool IsDefault(float t)
        {
            return t == 0;
        }
    }
}


