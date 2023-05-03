using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Entities;

namespace PainMod;

[Il2CppImplements(typeof(IComponentData))]
public struct MustBeDestroyedForOctopusLeaveStateCD
{
    
}
[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class MustBeDestroyedForOctopusLeaveStateCDAuthoring : ModCDAuthoringBase
{ public MustBeDestroyedForOctopusLeaveStateCDAuthoring(IntPtr ptr) : base(ptr) { }
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity,new MustBeDestroyedForOctopusLeaveStateCD());
    }
}