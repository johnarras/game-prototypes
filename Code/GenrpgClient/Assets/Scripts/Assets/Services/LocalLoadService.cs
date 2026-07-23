using OxDb.SharedCore.Interfaces;
using UnityEngine;

namespace OxDb.Client.Assets
{
    public interface ILocalLoadService : IInjectable
    {
        T LocalLoad<T>(string path);
    }

    public class LocalLoadService : ILocalLoadService
    {
        public T LocalLoad<T>(string path)
        {
            object obj = Resources.Load(path, typeof(T));
            return (T)obj;
        }
    }
}


