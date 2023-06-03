using Il2CppInterop.Runtime.Attributes;
using Unity.Entities;

namespace PainMod;
[Il2CppImplements(typeof(IComponentData))]
public struct OctopusModdedStateCD
{
    public float HpRatioToEnterState;
    public float HpRatioToEnterState2;
    public int iteration;
}