using MessagePack;
using System.Linq;

namespace OxDb.SharedCore.Utils.Data
{
    /// <summary>
    /// This is a small, densely-packed collection of integers to try to make savefiles smaller.
    /// Used for things like stats, currencies and tiles that should have most small integers
    /// used at most times.
    /// </summary>
    [MessagePackObject]
    public class SmallIdLongCollection : BaseSmallIdQuantityCollection<long>
    {
        [Key(0)] public long[] Data { get => _data; set => _data = value; }
        protected override long InternalAdd(long first, long second)
        {
            return first + second;
        }

        public override long GetAccumulation() { return (long)_data.Sum(); }
        protected override bool IsDefault(long t)
        {
            return t == 0;
        }
    }
}

