using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Doobers.Events
{
    public class SetDooberTarget
    {
        public long EntityTypeId { get; set; }
        public long EntityId { get; set; }
        public GameObject Target { get; set; }
    }
}
