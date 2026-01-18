using MessagePack;

namespace Genrpg.Shared.Utils.Data
{
    /// <summary>
    /// This is a small, densely-packed collection of integers to try to make savefiles smaller.
    /// Used for things like stats, currencies and tiles that should have most small integers
    /// used at most times.
    /// </summary>
    public class SmallIdIntCollection : BaseSmallIdQuantityCollection<int>
    {
        public int[] Data { get => _data; set => _data = value; }
        protected override int InternalAdd(int first, int second)
        {
            return first + second;
        }

        protected override bool IsDefault(int t)
        {
            return t == 0;
        }
    }
}

