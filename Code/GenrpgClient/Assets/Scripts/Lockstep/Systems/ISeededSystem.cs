using System;
using System.Collections.Generic;
using System.Text;
using Unity.Entities;

namespace Assets.Scripts.Lockstep.Systems
{
    public interface ISeededSystem
    {
        uint SystemId { get; }
    }
}
