using MessagePack;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdDoubleCollection : BaseSmallIdQuantityCollection<double>
    {
        [Key(0)] public double[] Data { get => _data; set => _data = value; }
        protected override double InternalAdd(double first, double second)
        {
            return first + second;
        }

        protected override bool IsDefault(double t)
        {
            return t == 0;
        }
    }
}


