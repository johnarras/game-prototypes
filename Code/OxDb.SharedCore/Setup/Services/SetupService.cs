using OxDb.SharedCore.Core.Entities;
using OxDb.SharedCore.GameSettings;
using OxDb.SharedCore.GameSettings.Services;
using OxDb.SharedCore.Interfaces;
using OxDb.SharedCore.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace OxDb.SharedCore.Setup.Services
{
    public class SetupService
    {

        List<string> _excludeTypeNames = new List<string>() { typeof(IExplicitInject).Name };

        List<Type> _ignoreBaseInterfaces = new List<Type>() { typeof(IExplicitInject), typeof(IInjectable), typeof(IInitializable) };

        public static readonly string[] ValidAssemblyPrefixes = new string[] { "OxDb." };

        public async Task Initialize(CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public virtual bool CreateMissingGameData()
        {
            return false;
        }

        public virtual async Task SetupGame(IGameState gs, List<object> existingObjects, CancellationToken token)
        {
            IReflectionService reflectionService = gs.loc.Get<IReflectionService>();
            List<string> completedAssemblyNames = new List<string>();
            SetupAssemblyServices(GetType().Assembly, gs.loc, reflectionService, completedAssemblyNames, token);
            gs.loc.ResolveSelf();
            gs.loc.Resolve(this);
            await reflectionService.InitializeServiceList(gs.loc, gs.loc.GetVals<IInjectable>(), token);

            foreach (object obj in existingObjects)
            {
                gs.loc.Resolve(obj);
            }
            IGameDataService gameDataService = gs.loc.Get<IGameDataService>();
            IGameData gameData = await gameDataService.LoadGameData();

        }

        private void SetupAssemblyServices(Assembly assembly, IServiceLocator loc, IReflectionService reflectionService, List<string> completedAssemblyNames, CancellationToken token)
        {
            if (completedAssemblyNames.Contains(assembly.GetName().Name))
            {
                return;
            }

            List<Assembly> assemblies = reflectionService.GetSearchAssemblies(assembly);

            AssemblyName[] dependencyAssemblyNames = assembly.GetReferencedAssemblies();

            List<AssemblyName> validDependencies = new List<AssemblyName>();
            foreach (AssemblyName dependencyAssemblyName in dependencyAssemblyNames)
            {

                foreach (string prefixName in ValidAssemblyPrefixes)
                {
                    if (dependencyAssemblyName.Name.IndexOf(prefixName) == 0)
                    {
                        validDependencies.Add(dependencyAssemblyName);
                    }
                }
            }

            foreach (AssemblyName validName in validDependencies)
            {
                Assembly dependency = assemblies.FirstOrDefault(x => x.GetName().Name == validName.Name);

                if (dependency != null)
                {
                    SetupAssemblyServices(dependency, loc, reflectionService, completedAssemblyNames, token);
                }
            }
            InjectAssemblyServices(assembly, loc, reflectionService, completedAssemblyNames, token);
            completedAssemblyNames.Add(assembly.GetName().Name);
        }

        private void InjectAssemblyServices(Assembly assembly, IServiceLocator loc, IReflectionService reflectionService, List<string> completedAssemblyNames, CancellationToken token)
        {
            List<Type> injectableTypes = reflectionService.GetTypesImplementing(assembly, typeof(IInjectable));

            foreach (Type type in injectableTypes)
            {
                bool excludeThis = false;
                foreach (string excludeName in _excludeTypeNames)
                {
                    if (type.GetInterface(excludeName) != null)
                    {
                        excludeThis = true;
                        break;
                    }
                }

                if (excludeThis)
                {
                    continue;
                }

                Type[] allInterfaces = type.GetInterfaces();

                object obj = Activator.CreateInstance(type);

                foreach (Type interfaceType in allInterfaces)
                {
                    if (_ignoreBaseInterfaces.Contains(interfaceType))
                    {
                        continue;
                    }

                    if (interfaceType.GetInterface(typeof(IInjectable).Name) == null)
                    {
                        continue;
                    }

                    loc.SetExplicitType(interfaceType, obj);
                }
            }
        }
    }
}


