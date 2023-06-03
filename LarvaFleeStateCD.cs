using Il2CppInterop.Runtime.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;

namespace PainMod
{
    [Il2CppImplements(typeof(IComponentData))]
    public struct LarvaFleeStateCD
    {
        public float fleeAtHealthRatio;
        public float leaveFleeAtHealthRatio;
        public float fleeSpeed;
    }
}
