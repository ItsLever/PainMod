using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Unity.Entities;

namespace PainMod;
[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class OctopusTenticleCounterCDAuthoring : ModCDAuthoringBase
{
    public OctopusTenticleCounterCDAuthoring(IntPtr ptr) : base(ptr) { }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new OctopusTenticleCounterCD());
    }
}