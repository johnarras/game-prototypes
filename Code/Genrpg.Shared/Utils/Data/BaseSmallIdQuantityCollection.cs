using System;
using System.Collections.Generic;

namespace Genrpg.Shared.Utils.Data
{
    /// <summary>
    /// This is a small, densely-packed collection of integers to try to make savefiles smaller.
    /// Used for things like stats, currencies and tiles that should have most small integers
    /// used at most times.
    /// </summary>
    /// 


    public abstract class BaseSmallIdObjectCollection<T> : BaseSmallIdQuantityCollection<T> where T : class, new()
    {
        protected override bool ExistsAtIndex(long index)
        {
            if (index >= _data.Length || IsDefault(_data[index]))
            {
                // The creation will throw the exception if the index is too big.
                this[index] = new T();
            }
            return true;
        }

        protected override bool IsDefault(T t)
        {
            return t == default(T);
        }
    }

    public abstract class BaseSmallIdQuantityCollection<T>
    {

        protected const int MaxSize = 256;

        protected T[] _data { get; set; } = new T[4];

        protected virtual bool ExistsAtIndex(long index)
        {
            return index >= 0 && index < _data.Length;
        }

        [MessagePack.IgnoreMember]
        public T this[long index]
        {

            get
            {
                if (index < 0 || index >= MaxSize)
                {
                    return default;
                }

                if (ExistsAtIndex(index))
                {
                    return _data[index];
                }
                return default;
            }
            set
            {
                if (index < 0 || index >= MaxSize)
                {
                    throw new IndexOutOfRangeException($"The index must be between 0 and {MaxSize - 1}");
                }

                if (_data.Length <= index)
                {
                    int size = Math.Max(2, _data.Length);
                    while (size <= index)
                    {
                        size *= 2;
                    }
                    T[] newData = new T[size];

                    for (int i = 0; i < _data.Length; i++)
                    {
                        newData[i] = _data[i];
                    }
                    _data = newData;
                }

                _data[index] = value;

            }
        }

        public IEnumerator<T> GetEnumerator() => ((IEnumerable<T>)_data).GetEnumerator();
        protected abstract T InternalAdd(T first, T second);
        protected abstract bool IsDefault(T t);


        public void Clear()
        {
            _data = new T[4];
        }

        public void Trim()
        {
            int maxNonzeroIndex = 0;

            for (int i = _data.Length - 1; i >= 0; i--)
            {
                if (!IsDefault(_data[i]))
                {
                    maxNonzeroIndex = i;
                    break;
                }
            }

            if (maxNonzeroIndex != _data.Length - 1)
            {
                T[] newData = new T[Math.Max(4, maxNonzeroIndex + 1)];
                for (int i = 0; i < maxNonzeroIndex + 1; i++)
                {
                    newData[i] = _data[i];
                }
                _data = newData;
            }
        }

        public void Add(long id, T val)
        {
            this[id] = InternalAdd(this[id], val);
        }
    }
}


