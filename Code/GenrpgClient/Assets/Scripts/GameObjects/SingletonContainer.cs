using Assets.Scripts.Core.Interfaces;
using OxDb.SharedCore.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public interface ISingletonContainer : IInitializable, IClientResetCleanup
    {
        public GameObject GetSingleton(string name);
        public GameObject GetAssetParent<T>() where T : class;
    }

    public class SingletonContainer : ISingletonContainer
    {
        private GameObject _root = null;
        private Dictionary<string, GameObject> _objectDict = new Dictionary<string, GameObject>();

        private IClientEntityService _clientEntityService = null;
        private IClientAppService _appService = null;

        public async Task Initialize(CancellationToken token)
        {
            token.Register(() => DestroyCreatedSingletons());


            await Task.CompletedTask;
        }

        private void DestroyCreatedSingletons()
        {
            foreach (GameObject go in _objectDict.Values)
            {
                _clientEntityService.Destroy(go);
            }
            _objectDict.Clear();
            _clientEntityService.DestroyAllChildren(_root);
        }

        public GameObject GetAssetParent<T>() where T : class
        {
            return GetSingleton(typeof(T).Name + "Parent");
        }

        public GameObject GetSingleton(string childName)
        {
            if (!_appService.IsPlaying)
            {
                return null;
            }

            if (_root == null)
            {
                _root = new GameObject() { name = "SingletonParent" };
                _objectDict[_root.name] = _root;
            }

            string fullName = childName;

            if (_objectDict.TryGetValue(fullName, out GameObject go))
            {
                return go;
            }

            GameObject newObj = new GameObject(childName);
            _clientEntityService.AddToParent(newObj, _root);
            _objectDict[fullName] = newObj;

            return newObj;
        }

        public async Task OnReset(CancellationToken token)
        {
            DestroyCreatedSingletons();
            await Task.CompletedTask;
        }
    }
}


