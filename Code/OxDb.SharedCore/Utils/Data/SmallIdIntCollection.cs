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
    public class SmallIdIntCollection : BaseSmallIdQuantityCollection<int>
    {
        [Key(0)] public int[] Data { get => _data; set => _data = value; }
        protected override int InternalAdd(int first, int second)
        {
            return first + second;
        }

        public override long GetAccumulation() { return (long)_data.Sum(); }
        protected override bool IsDefault(int t)
        {
            return t == 0;
        }
    }
}

