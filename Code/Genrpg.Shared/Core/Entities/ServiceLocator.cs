using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Genrpg.Shared.Core.Entities
{

    /// <summary>
    /// This is a DI/IOC object that I implemented myself to 
    /// be able to have more than one of these within a given program for 
    /// different contexts (Such as in an editor with multiple games or
    /// multiple environments open)
    /// </summary>

    public class ServiceLocator : IServiceLocator
    {


        private Dictionary<Type, TypeFieldInfo> _fieldCache = new Dictionary<Type, TypeFieldInfo>();
        public class TypeFieldInfo
        {
            public bool InitOnResolve { get; set; }
            public List<TypeField> Fields = new List<TypeField>();
        }

        public class TypeField
        {
            public FieldInfo Field { get; set; }
            public object Value { get; set; }
            public bool InitOnResolve { get; set; }
        }

        public ServiceLocator(ILogService logService, IReflectionService reflectionService, IGameData gameData)
        {
            _logService = logService;
            _reflectionService = reflectionService; 
            Set(logService);
            Set(reflectionService);
            Set(gameData);
        }

        private ILogService _logService = null;
        private IReflectionService _reflectionService = null;

        /// <summary>
        /// Internal storage indexed by type
        /// </summary>
        private Dictionary<Type, IInjectable> _typeDict = new Dictionary<Type, IInjectable>();
        /// <summary>
        /// Internal storage indexed by the name of the type
        /// </summary>
        private Dictionary<string, IInjectable> _nameDict = new Dictionary<string, IInjectable>();

        private List<object> _storedDictionaryItems = new List<object>();

        /// <summary>
        /// Returns an instance of type T
        /// </summary>
        /// <typeparam name="T">The type to be returned. An IFoo can return a FooImpl as long as FooImpl is IFoo</typeparam>
        /// <returns>An object of Type T</returns>
        public T Get<T>() where T : IInjectable
        {
            if (!typeof(T).IsInterface)
            {
                return default(T);
            }

            if (!_typeDict.ContainsKey(typeof(T)))
            {
                return default(T);
            }

            return (T)_typeDict[typeof(T)];
        }

        /// <summary>
        /// Get alist of all keys
        /// </summary>
        /// <returns>Returns a list of all keys from the Type dictionary.</returns>
        public List<Type> GetKeys()
        {
            List<Type> list = new List<Type>();
            if (_typeDict == null)
            {
                return list;
            }

            foreach (Type type in _typeDict.Keys)
            {
                list.Add(type);
            }
            return list;
        }

        public List<T> GetVals<T>()
        {
            List<T> retval = new List<T>();

            if (_typeDict == null)
            {
                return retval;
            }

            string interfaceName = typeof(T).Name;

            foreach (IInjectable injectable in _typeDict.Values)
            {
                if (injectable is T t && !retval.Contains(t))
                {
                    retval.Add(t);
                }
            }
            return retval;
        }

        /// <summary>
        /// Returns an object based on a type name
        /// </summary>
        /// <param name="typeName">The name of the type</param>
        /// <returns>An object which may or may not be of the correct type</returns>
        private object GetByName(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                return null;
            }

            if (!_nameDict.ContainsKey(typeName))
            {
                return null;
            }

            return _nameDict[typeName];
        }

        public void Set<T>(T obj) where T : IInjectable
        {
            SetExplicitType(typeof(T), obj);
        }

        /// <summary>
        /// Add an object to the dictionaries
        /// </summary>
        /// <typeparam name="T">The type to add</typeparam>
        /// <param name="obj">The object of type T to add</param>
        public void SetExplicitType(Type interfaceType, object obj)
        {
            if (obj == null || !(obj is IInjectable inj))
            {
                return;
            }

            if (obj.GetType().GetInterface(interfaceType.Name) == null)
            {
                _logService.Message("ServiceLocator added incompatible type: " + interfaceType.Name + " vs " + obj.GetType().Name);
                throw new Exception("ServiceLocator added incompatible type: " + interfaceType.Name + " vs " + obj.GetType().Name);
            }

            if (!interfaceType.IsInterface)
            {
                _logService.Message("ServiceLocator can only set Interfaces Not: " + interfaceType.Name + " ");
                throw new Exception("ServiceLocator: Attempted to Set non-interface type. " + interfaceType.Name);
            }

            if (_typeDict.ContainsKey(interfaceType))
            {
                _typeDict.Remove(interfaceType);
            }
            _typeDict[interfaceType] = inj;

            if (_nameDict.ContainsKey(interfaceType.Name))
            {
                _nameDict.Remove(interfaceType.Name);
            }

            _nameDict[interfaceType.Name] = inj;
        }

        string serviceTypeName = typeof(IInjectable).Name;
        string serviceLocatorTypeName = typeof(IServiceLocator).Name;
        string initOnResolveTypeName = typeof(IInitOnResolve).Name;

        private void ResolveFromFieldCache(object obj, TypeFieldInfo tfi)
        {
            foreach (TypeField tf in tfi.Fields)
            {
                if (tf.Value != null)
                {
                    tf.Field.SetValue(obj, tf.Value);
                }
                if (tf.InitOnResolve)
                {
                    Resolve(tf.Field.GetValue(obj));
                }
            }
            if (tfi.InitOnResolve)
            {
                IInitOnResolve initOnResolve = (IInitOnResolve)obj;
                initOnResolve.Init();
            }
        }

        public void Resolve(object obj)
        {
            if (obj == null)
            {
                return;
            }

            Type startType = obj.GetType();
            if (!startType.IsClass)
            {
                return;
            }

            if (_fieldCache.TryGetValue(startType, out TypeFieldInfo tfi))
            {
                ResolveFromFieldCache(obj, tfi);
                return;
            }

            Type currType = startType;
            tfi = new TypeFieldInfo();

            while (true)
            {
                FieldInfo[] fields = currType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsPublic || field.IsStatic)
                    {
                        continue;
                    }

                    Type fieldType = field.FieldType;

                    if (tfi.Fields.Any(x => x.Field.FieldType == fieldType))
                    {
                        continue;
                    }

                    TypeField tf = new TypeField()
                    {
                        Field = field,
                    };

                    if (fieldType.GetInterface(initOnResolveTypeName) != null)
                    {
                        tf.InitOnResolve = true;
                    }

                    if (fieldType.Name == serviceLocatorTypeName)
                    {
                        tf.Value = this;
                    }
                    else
                    {
                        Type serviceType = fieldType.GetInterface(serviceTypeName);
                        if (serviceType != null && fieldType.IsInterface)
                        {
                            tf.Value = GetByName(fieldType.Name);
                        }
                    }

                    if (tf.InitOnResolve || tf.Value != null)
                    {
                        tfi.Fields.Add(tf);
                    }
                }
                currType = currType.BaseType;
                if (!currType.IsClass || currType == typeof(object))
                {
                    break;
                }
            }

            if (startType.GetInterface(initOnResolveTypeName) != null)
            {
                tfi.InitOnResolve = true;
            }

            _fieldCache[startType] = tfi;

            ResolveFromFieldCache(obj, tfi);
        }

        public void ResolveSelf()
        {
            _fieldCache.Clear();
            foreach (object val in _typeDict.Values)
            {
                Resolve(val);
            }

            foreach (object val in _storedDictionaryItems)
            {
                Resolve(val);
            }
        }

        public void StoreDictionaryItem(object obj)
        {
            _storedDictionaryItems.Add(obj);
        }

    }
}


