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
            if (index >= MaxSize)
            {
                throw new IndexOutOfRangeException("Small Id Colletions are limited to size " + MaxSize);
            }
            if (index >= _data.Length)
            {
                T[] newData = new T[index + 1];

                for (int i = 0; i < _data.Length; i++)
                {
                    newData[i] = _data[i];
                }
                _data = newData;
            }
            if (IsDefault(_data[index]))
            {
                _data[index] = new T();
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

        public void CopyFrom(BaseSmallIdQuantityCollection<T> other)
        {
            _data = new T[other._data.Length];
            for (int i = 0; i < other._data.Length; i++)
            {
                _data[i] = other._data[i];
            }
        }


        protected const int MaxSize = 256;

        protected T[] _data { get; set; } = new T[4];
        public int Count() { return _data.Length; }

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
                    if (IsDefault(value))
                    {
                        return;
                    }

                    // Set this new size to exactly the size needed since these objects
                    // will be used in player data where we randomly add nonzero elements up to
                    // some small index amount, and then load and save the objects over and over,
                    // without changing the number of elements, so it's best to keep these 
                    // arrays as small as possible, even if there's a bit of extra copying
                    // the first time the array is being filled.
                    int size = (int)index + 1;

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
        public void Add(long id, T val)
        {
            this[id] = InternalAdd(this[id], val);
        }
    }
}


