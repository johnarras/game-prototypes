using Genrpg.Shared.Constants;
using Genrpg.Shared.DataStores.Interfaces;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.ProcGen.Settings.Names;
using Genrpg.Shared.Setup.Services;
using Genrpg.Shared.Stats.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Utils
{
    public class ReflectionUtils
    {

        private static List<Assembly> _searchAssemblies = new List<Assembly>();
        private static object _searchAssemblyLock = new object();

        public static Assembly[] GetAllAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        public static List<Type> GetTypesWithAttribute(Type attributeType)
        {
            List<Type> retval = new List<Type>();

            if (!typeof(Attribute).IsAssignableFrom(attributeType))
            {
                return retval;
            }

            List<Assembly> assemblies = GetSearchAssemblies(attributeType.Assembly);

            foreach (Assembly assembly in assemblies)
            {
                foreach (Type t in assembly.GetExportedTypes())
                {
                    if (!IsValidReflectionType(t))
                    {
                        continue;
                    }
                    if (t.GetCustomAttribute(attributeType) != null)
                    {
                        retval.Add(t);  
                    }
                }
            }

            return retval;
        }

        public static void AddSearchAssembly(Assembly assembly)
        {
            if (!_searchAssemblies.Contains(assembly))
            {
                lock (_searchAssemblyLock)
                {
                    if (!_searchAssemblies.Contains(assembly))
                    {
                        List<Assembly> newList = new List<Assembly>(_searchAssemblies);
                        newList.Add(assembly);

                        _searchAssemblies = newList;
                    }
                }
            }
        }

        private static List<Assembly> GetSearchAssemblies(Assembly typeAssembly)
        {

            List<Assembly> addAssemblies = new List<Assembly>();
            addAssemblies.Add(Assembly.GetExecutingAssembly());
            addAssemblies.Add(typeAssembly);

            foreach (Assembly executing in addAssemblies)
            {
                if (!_searchAssemblies.Contains(executing))
                {
                    lock (_searchAssemblyLock)
                    {
                        if (!_searchAssemblies.Contains(executing))
                        {
                            Assembly currentAssembly = executing;
                            int dotIndex = currentAssembly.FullName.IndexOf(".");

                            List<Assembly> newList = _searchAssemblies.ToList();
                            newList.Add(executing);

                            if (dotIndex > 0)
                            {
                                string prefix = currentAssembly.FullName.Substring(0, dotIndex);

                                Assembly[] assemblies = GetAllAssemblies();
                                foreach (Assembly assembly in assemblies)
                                {
                                    if (assembly.FullName.IndexOf(prefix) == 0)
                                    {
                                        newList.Add(assembly);
                                    }
                                }
                            }
                            _searchAssemblies = newList;
                        }
                    }
                }
            }

            return _searchAssemblies;
        }

        public static List<Type> GetTypesImplementing(Type interfaceType)
        {
            List<Type> retval = new List<Type>();
            if (interfaceType == null || !interfaceType.IsInterface)
            {
                return retval;
            }
            if (!interfaceType.IsInterface)
            {
                return retval;
            }


            List<Assembly> assemblies = GetSearchAssemblies(interfaceType.Assembly);

            foreach (Assembly assembly in assemblies)
            {
                retval.AddRange(GetTypesImplementing(assembly, interfaceType));
            }
            return retval;
        }

        private static readonly Type _excludeType = typeof(ExcludeFromReflectionAttribute);

        public static bool IsValidReflectionType(Type t)
        {
            return t.IsClass && !t.IsAbstract && !t.IsGenericType;
        }

        public static List<Type> GetTypesImplementing(Assembly assembly, Type interfaceType)
        {
            List<Type> retval = new List<Type>();
            foreach (Type t in assembly.GetExportedTypes())
            {

                if (!IsValidReflectionType(t))
                {
                    continue;
                }

                Type inter = t.GetInterface(interfaceType.Name);
                if (inter == null)
                {
                    continue;
                }

                if (Attribute.IsDefined(t, _excludeType))
                {
                    continue;
                }

                retval.Add(t);
            }
            return retval;
        }

        public static Dictionary<K, T> SetupDictionary<K, T>(IServiceLocator loc) where T : ISetupDictionaryItem<K>
        {
            Dictionary<K, T> dict = new Dictionary<K, T>();
            Type ttype = typeof(T);
            List<Type> types = GetTypesImplementing(typeof(T));

            foreach (Type t in types)
            {
                if (!IsValidReflectionType(t))
                {
                    continue;
                }

                T inst = (T)EntityUtils.DefaultConstructor(t);

                if (inst == null || inst.HelperKey == null)
                {
                    continue;
                }

                if (dict.ContainsKey(inst.HelperKey))
                {
                    dict.Remove(inst.HelperKey);
                }

                dict[inst.HelperKey] = inst;
                try
                {
                    loc.StoreDictionaryItem(inst);
                    loc.Resolve(inst);
                }
                catch (Exception e)
                {
                    Console.WriteLine("EXC: " + e.Message + " " + e.StackTrace);
                }
            }
            return dict;
        }

        public static async Task<object> CreateInstanceFromType(IServiceLocator loc, Type t, CancellationToken token)
        {
            object obj = Activator.CreateInstance(t);

            loc.Resolve(obj);

            if (obj is IInitializable service)
            {
                await InitializeServiceList(loc, new List<IInjectable> { service }, token);
            }

            return obj;
        }

        public static async Task InitializeServiceList(IServiceLocator loc, List<IInjectable> services, CancellationToken token)
        {
            List<IInitializable> setupServices = new List<IInitializable>();

            List<IPriorityInitializable> priorityServices = new List<IPriorityInitializable>();

            foreach (IInjectable service in services)
            {
                if (service is IInitializable setupService)
                {
                    setupServices.Add(setupService);
                }

                if (service is IPriorityInitializable prioritySetupService)
                {
                    priorityServices.Add(prioritySetupService);
                }
            }

            List<IGrouping<int, IPriorityInitializable>> groupedServices = priorityServices.GroupBy(x => x.SetupPriorityAscending()).OrderBy(x => x.Key).ToList();

            foreach (IGrouping<int, IPriorityInitializable> group in groupedServices)
            {

                List<Task> priorityTasks = new List<Task>();

                List<IPriorityInitializable> currentPriorityServices = group.ToList();

                foreach (IPriorityInitializable service in currentPriorityServices)
                {
                    priorityTasks.Add(service.PrioritySetup(token));
                }

                await Task.WhenAll(priorityTasks);

            }

            List<Task> setupTasks = new List<Task>();

            foreach (IInitializable setupService in setupServices)
            {
                setupTasks.Add(setupService.Initialize(token));
            }

            await Task.WhenAll(setupTasks);

        }

        public static List<KeyValue> GetStringConstants(Type t)
        {

            List<KeyValue> retval = new List<KeyValue>();

            List<FieldInfo> fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly) // Ensuring only constants
            .Where(f => f.FieldType == typeof(string)).ToList(); // Filtering for numeric types

            foreach (FieldInfo field in fields)
            {
                try
                {
                    retval.Add(new KeyValue()
                    {

                        Key = field.Name,
                        Val = (string)field.GetValue(null)
                    });
                }
                catch (Exception ex)
                {
                }
            }

            return retval;
        }

        public static List<NameValue> GetNumericConstants(Type t)
        {

            List<NameValue> retval = new List<NameValue>();

            List<FieldInfo> fields = t.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(f => f.IsLiteral && !f.IsInitOnly) // Ensuring only constants
            .Where(f => f.FieldType.IsPrimitive).ToList(); // Filtering for numeric types

            foreach (FieldInfo field in fields)
            {
                try
                {
                    retval.Add(new NameValue()
                    {
                        IdKey = (long)field.GetValue(null),
                        Name = field.Name,
                    });
                }
                catch (Exception ex)
                {
                    try
                    {
                        retval.Add(new NameValue()
                        {
                            IdKey = (int)field.GetValue(null),
                            Name = field.Name,
                        });
                    }
                    catch (Exception ex2)
                    {
                        Console.WriteLine(ex2.ToString() + " " + ex2.StackTrace + " Parent: " + ex.Message);
                    }
                }
            }

            return retval;
        }
    }
}


