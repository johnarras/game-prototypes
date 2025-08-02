using Genrpg.Shared.BoardGame.Upgrades.Services;
using Genrpg.Shared.Charms.Services;
using Genrpg.Shared.Crafting.Services;
using Genrpg.Shared.Entities.Services;
using Genrpg.Shared.Factions.Services;
using Genrpg.Shared.Ftue.Services;
using Genrpg.Shared.Interfaces;
using Genrpg.Shared.Inventory.Services;
using Genrpg.Shared.LoadSave.Services;
using Genrpg.Shared.MapServer.Services;
using Genrpg.Shared.Names.Services;
using Genrpg.Shared.Pathfinding.Services;
using Genrpg.Shared.PlayMultiplier.Services;
using Genrpg.Shared.ProcGen.Services;
using Genrpg.Shared.Quests.Services;
using Genrpg.Shared.Rewards.Services;
using Genrpg.Shared.SpellCrafting.Services;
using Genrpg.Shared.Spells.Services;
using Genrpg.Shared.Stats.Services;
using Genrpg.Shared.Tasks.Services;
using Genrpg.Shared.UnitEffects.Services;
using Genrpg.Shared.Units.Services;
using Genrpg.Shared.UserAbilities.Services;
using Genrpg.Shared.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Genrpg.Shared.Setup.Services
{
    public class SetupService
    {

        List<string> _excludeTypeNames = new List<string>() { typeof(IExplicitInject).Name };

        List<Type> _ignoreBaseInterfaces = new List<Type>() { typeof(IExplicitInject), typeof(IInjectable), typeof(IInitializable) };

        protected string[] _assemblyPrefixes = new string[] { "Genrpg." };

        public async Task Initialize( CancellationToken toke)
        {
            await Task.CompletedTask;
        }

        public virtual bool CreateMissingGameData()
        {
            return false;
        }

        public virtual async Task SetupGame(IServiceLocator loc, CancellationToken token)
        {
            List<string> completedAssemblyNames = new List<string>();
            SetupAssemblyServices(GetType().Assembly, loc, completedAssemblyNames, token);
            loc.ResolveSelf();
            loc.Resolve(this);
            await ReflectionUtils.InitializeServiceList(loc, loc.GetVals(), token);

        }

        private void SetupAssemblyServices(Assembly assembly, IServiceLocator loc, List<string> completedAssemblyNames, CancellationToken token)
        {
            if (completedAssemblyNames.Contains(assembly.GetName().Name))
            {
                return;
            }

            AssemblyName[] dependencyAssemblyNames = assembly.GetReferencedAssemblies();

            Assembly[] allAssemblies = ReflectionUtils.GetAllAssemblies();

            List<AssemblyName> validDependencies = new List<AssemblyName>();
            foreach (AssemblyName dependencyAssemblyName in dependencyAssemblyNames)
            {

                foreach (string prefixName in _assemblyPrefixes)
                {
                    if (dependencyAssemblyName.Name.IndexOf(prefixName) == 0)
                    {
                        validDependencies.Add(dependencyAssemblyName);
                    }
                }
            }

            foreach (AssemblyName validName in validDependencies)
            {
                Assembly dependency = allAssemblies.FirstOrDefault(x => x.GetName().Name == validName.Name);

                if (dependency != null)
                {
                    SetupAssemblyServices(dependency, loc, completedAssemblyNames, token);
                }
            }
            InjectAssemblyServices(assembly, loc, completedAssemblyNames, token);  
            completedAssemblyNames.Add(assembly.GetName().Name);
        }

        private void InjectAssemblyServices(Assembly assembly, IServiceLocator loc, List<string> completedAssemblyNames, CancellationToken token)
        {
                
            List<Type> injectableTypes = ReflectionUtils.GetTypesImplementing(assembly, typeof(IInjectable));

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
