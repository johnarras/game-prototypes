using MessagePack;
using Genrpg.Shared.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;
using Genrpg.Shared.Entities.Utils;
using Genrpg.Shared.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Genrpg.Shared.ProcGen.Settings.Names;

namespace Genrpg.Shared.Utils
{
    [MessagePackObject]
    public class ReflectionUtils
    {

        private static Assembly[] _allAssemblies = AppDomain.CurrentDomain.GetAssemblies();
        private static List<Assembly> _searchAssemblies = new List<Assembly>();

        public static Assembly[] GetAllAssemblies()
        {
            return _allAssemblies;
        }

        public static void AddAllowedAssembly(Assembly assembly)
        {
            if (!_searchAssemblies.Contains(assembly))
            {
                _searchAssemblies.Add(assembly);
            }
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

            if (!_searchAssemblies.Contains(Assembly.GetExecutingAssembly()))
            {
                Assembly currentAssembly = Assembly.GetExecutingAssembly();
                int dotIndex = currentAssembly.FullName.IndexOf(".");

                if (dotIndex > 0)
                {
                    string prefix = currentAssembly.FullName.Substring(0, dotIndex);

                    Assembly[] assemblies = _allAssemblies;
                    foreach (Assembly assembly in assemblies)
                    {
                        if (assembly.FullName.IndexOf(prefix) == 0)
                        {
                            AddAllowedAssembly(assembly);
                        }
                    }
                }
            }

            foreach (Assembly assembly in _searchAssemblies)
            {
                retval.AddRange(GetTypesImplementing(assembly, interfaceType));
            }
            return retval;
        }

        public static List<Type> GetTypesImplementing(Assembly assembly, Type interfaceType)
        {
            List<Type> retval = new List<Type>();
            foreach (Type t in assembly.GetExportedTypes())
            {
                if (!t.IsClass)
                {
                    continue;
                }

                if (t.IsAbstract)
                {
                    continue;
                }

                if (t.IsGenericType)
                {
                    continue;
                }

                Type inter = t.GetInterface(interfaceType.Name);
                if (inter == null)
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

            Assembly[] assemblies = _allAssemblies;

            foreach (Assembly assembly in assemblies)
            {
                if (assembly.FullName.IndexOf(Game.Prefix) < 0
                    && assembly.FullName.IndexOf(Game.DefaultPrefix) < 0
                    && !_searchAssemblies.Contains(assembly))
                {
                    continue;
                }

                foreach (Type t in assembly.GetExportedTypes())
                {
                    if (!t.IsClass)
                    {
                        continue;
                    }

                    if (t.IsAbstract)
                    {
                        continue;
                    }

                    if (t.ContainsGenericParameters)
                    {
                        continue;
                    }

                    Type inter = t.GetInterface(ttype.Name);
                    if (inter == null)
                    {
                        continue;
                    }

                    T inst = (T)EntityUtils.DefaultConstructor(t);

                    if (inst == null || inst.Key == null)
                    {
                        continue;
                    }

                    if (dict.ContainsKey(inst.Key))
                    {
                        dict.Remove(inst.Key);
                    }

                    dict[inst.Key] = inst;
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

            List<IGrouping<int,IPriorityInitializable>> groupedServices = priorityServices.GroupBy(x => x.SetupPriorityAscending()).OrderBy(x=>x.Key).ToList();  

            foreach (IGrouping<int,IPriorityInitializable> group in groupedServices)
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

            await loc.InitializeDictionaryItems(token);

            await Task.WhenAll(setupTasks);

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
                        Console.WriteLine(ex.ToString() + " " + ex.StackTrace);
                    }
                }
            }

            return retval;
        }
    }
}
