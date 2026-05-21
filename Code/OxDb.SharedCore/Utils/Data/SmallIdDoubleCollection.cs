using System.Linq;

namespace OxDb.SharedCore.Utils.Data
{
    public class SmallIdDoubleCollection : BaseSmallIdQuantityCollection<double>
    {
        public double[] Data { get => _data; set => _data = value; }
        protected override double InternalAdd(double first, double second)
        {
            return first + second;
        }

        public override long GetAccumulation() { return (long)_data.Sum(); }
        protected override bool IsDefault(double t)
        {
            return t == 0;
        }
    }
}


