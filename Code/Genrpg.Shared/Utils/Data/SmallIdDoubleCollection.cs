using MessagePack;

namespace Genrpg.Shared.Utils.Data
{
    public class SmallIdDoubleCollection : BaseSmallIdQuantityCollection<double>
    {
        public override double[] Data { get; set; } = new double[4];
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


