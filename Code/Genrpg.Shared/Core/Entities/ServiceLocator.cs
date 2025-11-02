using Genrpg.Shared.Analytics.Services;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.GameSettings;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Logging.Interfaces;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Genrpg.Shared.Core.Entities
{

    /// <summary>
    /// This is a DI/IOC object that I implemented myself to 
    /// be able to have more than one of these within a given program for 
    /// different contexts (Such as in an editor with multiple games or
    /// multiple environments open)
    /// </summary>

    // MessagePackIgnore
    public class ServiceLocator : IServiceLocator
    {

        public ServiceLocator(ITextSerializer serializer, ILogService logService, IAnalyticsService analyticsService, IGameData gameData)
        {
            _logService = logService;

            Set(serializer);
            Set(logService);
            Set(analyticsService);
            Set(gameData);
        }

        private ILogService _logService = null;

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

        public List<T> GetVals<T>() where T : IInjectable
        {
            List<T> retval = new List<T>();

            if (_typeDict == null)
            {
                return retval;
            }

            string interfaceName = typeof(T).Name;

            foreach (IInjectable injectable in _typeDict.Values)
            {
                if (injectable is T t)
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

        public void Resolve(object obj)
        {
            if (obj == null)
            {
                return;
            }
            Type type = obj.GetType();
            if (!type.IsClass)
            {
                return;
            }

            string serviceTypeName = typeof(IInjectable).Name;
            string serviceLocatorTypeName = typeof(IServiceLocator).Name;
            string initOnResolveTypeName = typeof(IInitOnResolve).Name;

            while (true)
            {
                FieldInfo[] fields = type.GetFields(BindingFlags.NonPublic | BindingFlags.Instance);

                foreach (FieldInfo field in fields)
                {
                    if (field.IsPublic || field.IsStatic)
                    {
                        continue;
                    }

                    Type fieldType = field.FieldType;


                    if (fieldType.GetInterface(initOnResolveTypeName) != null)
                    {
                        Resolve(EntityUtils.GetObjectValue(obj, field));
                    }

                    if (fieldType.Name == serviceLocatorTypeName)
                    {
                        EntityUtils.SetObjectValue(obj, field, this);
                        continue;
                    }

                    Type serviceType = fieldType.GetInterface(serviceTypeName);
                    if (serviceType == null)
                    {
                        continue;
                    }

                    if (!fieldType.IsInterface)
                    {
                        continue;
                    }

                    object serviceObject = GetByName(fieldType.Name);
                    if (serviceObject == null)
                    {
                        continue;
                    }

                    EntityUtils.SetObjectValue(obj, field, serviceObject);

                }
                type = type.BaseType;
                if (!type.IsClass || type == typeof(object))
                {
                    break;
                }
            }

            if (obj.GetType().GetInterface(initOnResolveTypeName) != null)
            {
                IInitOnResolve initOnResolve = (IInitOnResolve)obj;
                initOnResolve.Init();
            }
        }

        public void ResolveSelf()
        {
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
