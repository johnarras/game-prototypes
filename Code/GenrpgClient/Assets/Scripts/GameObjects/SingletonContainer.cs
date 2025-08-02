using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.GameObjects
{
    public interface ISingletonContainer : IInitializable
    {
        public GameObject GetSingleton(string name);
    }

    public class SingletonContainer : ISingletonContainer
    {
        private GameObject _root = null;
        private Dictionary<string, GameObject> _objectDict = new Dictionary<string, GameObject>();

        private IClientEntityService _clientEntityService = null;
        private IInitClient _initClient = null;
        private IClientAppService _appService = null;

        string containerName = "InitClient";

        public async Task Initialize(CancellationToken token)
        {
            token.Register(() => DestroyCreatedSingletons());

            if (_root == null)
            {
                _root = (GameObject)_initClient.GetRootObject();

                if (_root == null)
                {
                    _root = new GameObject() { name = containerName };
                }
            }

            await Task.CompletedTask;
        }

        private void DestroyCreatedSingletons()
        {
            foreach (GameObject go in _objectDict.Values)
            {
                _clientEntityService.Destroy(go);
            }
        }


        public GameObject GetSingleton(string name)
        {

            if (!_appService.IsPlaying)
            {
                return null;
            }

            if (_objectDict.TryGetValue(name, out GameObject go))
            {
                return go;
            }

            go = new GameObject(name);
            _clientEntityService.AddToParent(go, _root);
            _objectDict[name] = go;
            return go;
        }
    }
}
