using System;
using System.Collections.Generic;
using Unity.Entities;

namespace OxDb.Client.Lockstep.Config.Core
{
    public class SimulationPipeline
    {
        private readonly List<Type> _systems = new List<Type>();
        public IReadOnlyList<Type> Systems => _systems;

        // Constraint: T must be a struct (unmanaged system) and implement ISystem
        public SimulationPipeline AddStep<T>() where T : unmanaged, ISystem
        {
            _systems.Add(typeof(T));
            return this;
        }

        // Support for SystemBase (managed systems) if you need them
        public SimulationPipeline AddManagedStep<T>() where T : SystemBase
        {
            _systems.Add(typeof(T));
            return this;
        }
    }
}