using OxDb.Client.Assets.ObjectPools;
using OxDb.Client.Awaitables;
using OxDb.Client.UI.Interfaces;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Logalytics.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;

namespace OxDb.Client.GameObjects
{

    public interface IClientEntityService : IInjectable
    {
        C FullInstantiate<C>(C c) where C : class;
        object FullInstantiate(object obj);
        void InitializeHierarchy(object obj);
        C GetOrAddComponent<C>(object obj) where C : class;
        void SetActive(object obj, bool value);
        void Destroy(object obj);
        void WaitToDestroy(object obj, float waitSeconds, CancellationToken token);
        object FindChild(object objIn, string name);
        List<T> GetComponents<T>(object obj);
        T GetInterface<T>(object obj);
        T GetComponent<T>(object obj) where T : class;
        T FindInParents<T>(object obj) where T : class;
        void DestroyAllChildren(object obj);
        void SetLayer(object obj, string layerName);
        void SetLayer(object obj, int layer);
        void AddToParent(object childObj, object parentObj);
        object GetEntity(object obj);
        void RegisterDestroyCallback(object obj, Action action);

        void ReorderSiblings<T>(List<T> objects) where T : UnityEngine.Object;

        TChild GetChildComponentOfParent<TParent, TChild>(object obj) where TChild : class where TParent : class;
    }



    public class ClientEntityService : IClientEntityService
    {
        protected IServiceLocator _loc = null;
        protected IInitClient _initClient = null;
        protected IUIService _uiService = null;
        protected ILogService _logService = null;
        protected IAwaitableService _awaitableService = null;
#if UNITY_EDITOR
        private IClientAppService _clientAppService = null!;
#endif


        public C FullInstantiate<C>(C obj) where C : class
        {
            if (!(obj is UnityEngine.Object cobj))
            {
                return null;
            }

            UnityEngine.Object cdupe = GameObject.Instantiate(cobj);

            cdupe.name = cdupe.name.Replace("(Clone)", "");
            if (cdupe is Component cdupeComp)
            {
                InitializeHierarchy(cdupeComp.gameObject);
            }
            return cdupe as C;
        }

        public object FullInstantiate(object obj)
        {
            if (!(obj is GameObject go))
            {
                if (obj is UnityEngine.Object uobj)
                {
                    return FullInstantiate(uobj);
                }
                return null;
            }

            GameObject dupe = GameObject.Instantiate(go);
            dupe.name = dupe.name.Replace("(Clone)", "");
            InitializeHierarchy(dupe);
            return dupe;
        }

        public void InitializeHierarchy(object obj)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            SetActive(go, true);
            List<Component> allComponents = GetComponents<Component>(go);

            for (int b = allComponents.Count - 1; b >= 0; b--)
            {
                Component comp = allComponents[b];
                if (comp is BaseBehaviour baseBehaviour)
                {
                    try
                    {
                        _loc.Resolve(baseBehaviour);
                    }
                    catch (Exception e)
                    {
                        _logService.Exception(e, "InitializeHierarchy");
                    }
                }
                else if (comp is GText gtext && !string.IsNullOrEmpty(gtext.text))
                {
                    _uiService.SetText(gtext, gtext.text);
                }
                else if (comp is MeshRenderer rend)
                {
                    rend.lightProbeUsage = LightProbeUsage.Off;
                    rend.reflectionProbeUsage = ReflectionProbeUsage.Off;
                    rend.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
                    rend.allowOcclusionWhenDynamic = false;
                }
            }
        }

        public C GetOrAddComponent<C>(object obj) where C : class
        {
            if (!(obj is GameObject go))
            {
                return null;
            }

            if (!go.TryGetComponent<C>(out C c))
            {
                c = go.AddComponent(typeof(C)) as C;
            }
#if UNITY_EDITOR
            go.hideFlags = 0;
#endif

            if (c is BaseBehaviour bb)
            {
                InitializeHierarchy(go);
            }
            return c;
        }

        public void SetActive(object obj, bool value)
        {

            GameObject go = obj as GameObject;
            if (go == null)
            {
                if (obj is Component comp)
                {
                    go = comp.gameObject;
                }
            }

            if (!go.Equals(null) && go.activeSelf != value)
            {
                go.SetActive(value);
            }
        }

        public void Destroy(object obj)
        {
            if (!(obj is UnityEngine.Object unityObject))
            {
                return;
            }

            MonoBehaviour mb = unityObject as MonoBehaviour;

            if (mb != null)
            {
                unityObject = mb.gameObject;
            }


#if UNITY_EDITOR
            if (!_clientAppService.IsPlaying)
            {
                GameObject.DestroyImmediate(unityObject);
            }
            else
            {
                GameObject.Destroy(unityObject);
            }
#else
        GameObject.Destroy(unityObject);
#endif
        }

        public void WaitToDestroy(object obj, float time, CancellationToken token)
        {
            _awaitableService.ForgetAwaitable(DelayDestroyInternal(obj, time, token));
        }

        private async Awaitable DelayDestroyInternal(object obj, float time, CancellationToken token)
        {
            await Awaitable.WaitForSecondsAsync(time, token);
            await Awaitable.MainThreadAsync();
            Destroy(obj);
        }

        public object FindChild(object objIn, string name)
        {
            GameObject go = objIn as GameObject;
            if (go == null)
            {
                return null;
            }

            if (go == null || string.IsNullOrEmpty(name))
            {
                return null;
            }

            if (go.name == name)
            {
                return go;
            }

            for (int t = 0; t < go.transform.childCount; t++)
            {

                GameObject obj2 = go.transform.GetChild(t).gameObject;
                if (obj2.name == name)
                {
                    return obj2;
                }
            }
            for (int t = 0; t < go.transform.childCount; t++)
            {
                GameObject obj2 = (GameObject)FindChild(go.transform.GetChild(t).gameObject, name);
                if (obj2 != null)
                {
                    return obj2;
                }
            }
            return null;
        }

        public List<T> GetComponents<T>(object obj)
        {
            List<T> comps = new List<T>();

            if (!(obj is GameObject go))
            {
                if (obj is MonoBehaviour mb)
                {
                    go = mb.gameObject;
                }
                else
                {
                    return new List<T>();
                }
            }

            if (go == null)
            {
                return comps;
            }

            T[] arr = go.GetComponentsInChildren<T>(true);
            if (arr != null && arr.Length > 0)
            {
                foreach (T comp in arr)
                {
                    comps.Add(comp);
                }
            }

            return comps;

        }

        public T GetComponent<T>(object obj) where T : class
        {
            if (!(obj is GameObject go))
            {
                if (obj is MonoBehaviour mb)
                {
                    go = mb.gameObject;
                }
                else
                {
                    return default(T);
                }
            }

            go.TryGetComponent<T>(out T comp);

            if (comp != null)
            {
                return comp;
            }

            T[] comps = go.GetComponentsInChildren<T>(true);

            if (comps != null && comps.Length > 0)
            {
                return comps[0];
            }
            return default(T);
        }

        public T FindInParents<T>(object obj) where T : class
        {
            GameObject go = obj as GameObject;
            if (go == null)
            {
                if (obj is MonoBehaviour mb)
                {
                    go = mb.gameObject;
                }
                else
                {
                    return null;
                }
            }

            if (typeof(T).IsSubclassOf(typeof(Component)))
            {
                if (go.TryGetComponent<T>(out T comp))
                {
                    return comp;
                }
                else
                {
                    Transform t = go.transform.parent;

                    while (t != null && comp == null)
                    {
                        comp = t.gameObject.GetComponent<T>();

                        if (comp != null)
                        {
                            return comp;
                        }
                        t = t.parent;
                    }
                    return default(T);
                }
            }
            else
            {
                while (go != null)
                {
                    Component[] comps = go.GetComponents<Component>();
                    foreach (Component comp in comps)
                    {
                        if (comp is T t)
                        {
                            return t;
                        }
                    }
                    if (go.transform.parent != null)
                    {
                        go = go.transform.parent.gameObject;
                    }
                }
            }
            return default(T);
        }

        public void DestroyAllChildren(object obj)
        {
            if (!(obj is GameObject go))
            {
                return;
            }


            // Iterate backwards from the last child to the first (index 0)
            // This is safe because destroying a child removes it from the list,
            // shifting the subsequent elements' indices up.
            for (int i = go.transform.childCount - 1; i >= 0; i--)
            {
                // Get the child Transform
                Transform child = go.transform.GetChild(i);

                // Call Destroy() on the child's GameObject.
                // DO NOT use DestroyImmediate() at runtime.
                DestroyAllChildren(child.gameObject);
                Destroy(child.gameObject);
            }
        }

        public void SetLayer(object obj, string layerName)
        {
            SetLayer(obj, LayerUtils.NameToLayer(layerName));

        }

        public void SetLayer(object obj, int layer)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            if (go == null || layer < 0 || layer >= 32)
            {
                return;
            }

            InnerSetLayerRecursive(go, layer);
        }

        private void InnerSetLayerRecursive(object obj, int layer)
        {
            if (!(obj is GameObject go))
            {
                return;
            }

            if (go == null)
            {
                return;
            }

            go.layer = layer;
            foreach (Transform tr in go.transform)
            {
                InnerSetLayerRecursive(tr.gameObject, layer);
            }
        }



        public void AddToParent(object childObjIn, object parentObjIn)
        {
            GameObject childObj = childObjIn as GameObject;
            GameObject parentObj = parentObjIn as GameObject;

            if (childObj == null)
            {
                if (childObjIn is MonoBehaviour mb)
                {
                    childObj = mb.gameObject;
                }
                else
                {
                    return;
                }
            }

            if (parentObj == null)
            {
                if (parentObjIn is MonoBehaviour mb)
                {
                    parentObj = mb.gameObject;
                }
                else
                {

                    return;
                }
            }


            childObj.transform.SetParent(parentObj.transform);

            childObj.transform.localEulerAngles = Vector3.zero;
            childObj.transform.localScale = Vector3.one;
            SetLayer(childObj, parentObj.layer);
            childObj.transform.localPosition = Vector3.zero;
        }

        public object GetEntity(object obj)
        {
            if (obj is MonoBehaviour mb)
            {
                return mb.gameObject;
            }
            return null;
        }

        public void RegisterDestroyCallback(object obj, Action action)
        {
            MonoBehaviour mb = obj as MonoBehaviour;

            if (mb == null)
            {
                GameObject go = obj as GameObject;

                if (go == null)
                {
                    return;
                }

                mb = GetComponent<MonoBehaviour>(go);
            }

            if (mb == null)
            {
                return;
            }

            if (action != null && mb is IDestroyCallback dc)
            {
                dc.SetDestroyCallback(action);
            }
        }

        public T GetInterface<T>(object obj)
        {
            List<MonoBehaviour> behaviours = GetComponents<MonoBehaviour>(obj);

            foreach (MonoBehaviour mb in behaviours)
            {
                if (mb is T t)
                {
                    return t;
                }
            }

            return default(T);
        }

        public void ReorderSiblings<T>(List<T> objects) where T : UnityEngine.Object
        {
            for (int i = 0; i < objects.Count; i++)
            {
                if (objects[i] is GameObject go)
                {
                    go.transform.SetSiblingIndex(i);
                }
                else if (objects[i] is MonoBehaviour mb)
                {
                    mb.transform.SetSiblingIndex(i);
                }
            }
        }

        public TChild GetChildComponentOfParent<TParent, TChild>(object obj)
            where TParent : class
            where TChild : class
        {

            return GetComponent<TChild>(FindInParents<TParent>(obj));
        }
    }
}


