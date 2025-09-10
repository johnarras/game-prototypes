using Assets.Scripts.Assets.Entities;
using Assets.Scripts.Awaitables;
using Assets.Scripts.GameObjects;
using Genrpg.Shared.Client.Core;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Assets.ObjectPools
{
    public class ObjectPool : IInitOnResolve
    {
        protected IAssetService _assetService = null;
        protected IInitClient _initClient = null;
        protected IClientEntityService _clientEntityService = null;
        protected ILogService _logService = null;
        protected ISingletonContainer _singletonContainer = null;
        protected IAwaitableService _awaitableService = null;


        protected GameObject _globalAssetParent = null;

        protected Dictionary<string, List<GameObject>> _cache = new Dictionary<string, List<GameObject>>();

        protected Dictionary<string, List<GameObject>> _activeObjects = new Dictionary<string, List<GameObject>>();

        public void Init()
        {
            _globalAssetParent = _singletonContainer.GetSingleton(AssetConstants.GlobalAssetParent);
        }

        public void Clear()
        {
            foreach (string key in _cache.Keys)
            {
                foreach (GameObject go in _cache[key])
                {
                    _clientEntityService.Destroy(go);
                }
            }
            _cache.Clear();
            foreach (string key in _activeObjects.Keys)
            {
                foreach (GameObject obj in _activeObjects[key])
                {
                    _clientEntityService.Destroy(obj);
                }
            }
            _activeObjects.Clear();
        }


        public void ReturnObject(object obj)
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                MonoBehaviour mb = obj as MonoBehaviour;
                if (mb != null)
                {
                    go = mb.gameObject;
                }
            }

            if (go == null)
            {
                return;
            }


            foreach (string key in _activeObjects.Keys)
            {
                if (_activeObjects[key].Contains(go))
                {
                    _activeObjects[key].Remove(go);
                    if (!_cache.ContainsKey(key))
                    {
                        _cache[key] = new List<GameObject>();
                    }
                    _cache[key].Add(go);
                    _clientEntityService.AddToParent(go, _globalAssetParent);
                    _clientEntityService.SetActive(go, false);
                    return;
                }
            }

            // GameObject was not in active objects, do destroy it.
            _clientEntityService.Destroy(go);
        }

        public void CheckoutObject(object parent, string assetCategory, string assetPath,
            OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null)
        {
            _awaitableService.ForgetTask(CheckoutObjectAsync(parent, assetCategory, assetPath, handler, data, token, subdirectory));
        }

        public async Task<T> CheckoutObjectAsync<T>(object parent, string assetCategory, string assetPath,
            OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null) where T : MonoBehaviour
        {
            GameObject obj = await CheckoutObjectAsync(parent, assetCategory, assetPath, handler, data, token, subdirectory);

            if (obj == null)
            {
                return default(T);
            }

            return _clientEntityService.GetComponent<T>(obj);
        }


        public async Task<GameObject> CheckoutObjectAsync(object parent, string assetCategory, string assetPath,
            OnDownloadHandler handler, object data, CancellationToken token, string subdirectory = null)
        {

            string fullAssetCategory = assetCategory;
            if (!string.IsNullOrEmpty(subdirectory))
            {
                fullAssetCategory += "/" + subdirectory;
            }

            string bundleName = _assetService.GetBundleNameForCategoryAndAsset(fullAssetCategory, assetPath);

            string fullName = bundleName + "/" + assetPath;

            GameObject newItem = null;
            if (_cache.TryGetValue(fullName, out List<GameObject> cachedItems))
            {
                if (cachedItems.Count > 0)
                {
                    newItem = cachedItems[cachedItems.Count - 1];
                    cachedItems.RemoveAt(cachedItems.Count - 1);
                }
            }

            if (newItem == null)
            {
                newItem = (GameObject)(await _assetService.LoadAssetAsync(assetCategory, assetPath, parent, token, subdirectory));
            }

            if (newItem == null)
            {
                return null;
            }

            if (parent == null)
            {
                newItem.transform.parent = null;
            }
            else
            {
                _clientEntityService.AddToParent(newItem, parent);
            }

            _clientEntityService.SetActive(newItem, true);
            if (handler != null)
            {
                handler(newItem, data, token);
            }

            if (!_activeObjects.ContainsKey(fullName))
            {
                _activeObjects[fullName] = new List<GameObject>();
            }
            _activeObjects[fullName].Add(newItem);
            return newItem;
        }
    }
}