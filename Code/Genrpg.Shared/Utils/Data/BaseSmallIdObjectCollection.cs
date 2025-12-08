using MessagePack;
using System;

namespace Genrpg.Shared.Utils.Data
{
    public abstract class BaseSmallIdObjectCollection<T> where T : class, new()
    {

        protected const int MaxSize = 64;
        [IgnoreMember] public abstract T[] Data { get; set; }


        protected virtual bool IsDefault(T t)
        {
            return t == default(T);
        }


        public void Clear()
        {
            Data = new T[4];
        }

        public int GetLength()
        {
            return Data.Length;
        }

        public void Trim()
        {
            int maxNonzeroIndex = 0;

            for (int i = 0; i < Data.Length; i++)
            {
                if (!IsDefault(Data[i]))
                {
                    maxNonzeroIndex = i;
                }
            }

            T[] newData = new T[Math.Max(4, maxNonzeroIndex + 1)];
            for (int i = 0; i < maxNonzeroIndex + 1; i++)
            {
                newData[i] = Data[i];
            }
            Data = newData;
        }

        public T Get(long id)
        {
            if (id < 1 || id >= MaxSize)
            {
                throw new Exception($"CollectionContainer id must be between 1 and {MaxSize - 1}.");
            }
            if (id > Data.Length)
            {
                Set(id, new T());
            }
            id--;
            if (Data[id] == default(T))
            {
                Data[id] = new T();
            }

            return Data[id];
        }

        protected void Set(long id, T val)
        {
            if (id < 1 || id >= MaxSize)
            {
                throw new Exception($"CollectionContainer id must be between 1 and {MaxSize - 1}.");
            }

            id--;
            if (id < Data.Length)
            {
                Data[id] = val;
                if (val == null)
                {
                    Trim();
                }
                return;
            }

            if (val == null)
            {
                return;
            }

            T[] newData = new T[id + 1];

            for (int i = 0; i < Data.Length; i++)
            {
                newData[i] = Data[i];
            }

            newData[id] = val;
            Data = newData;

            Data[id] = val;
        }

        public bool Has(long id)
        {
            return !IsDefault(Get(id));
        }
    }
}
