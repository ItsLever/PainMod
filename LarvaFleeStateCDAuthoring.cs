using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Entities;
using Unity.Mathematics;

namespace PainMod;
[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class LarvaFleeStateCDAuthoring : ModCDAuthoringBase
{
    public Il2CppValueField<float> hpToFlee;
    public Il2CppValueField<float> hpToLeaveFlee;
    public Il2CppValueField<float> speedToFlee;

    public LarvaFleeStateCDAuthoring(IntPtr ptr) : base(ptr) { }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new LarvaFleeStateCD()
        {
            fleeAtHealthRatio = hpToFlee,
            leaveFleeAtHealthRatio = hpToLeaveFlee,
            fleeSpeed = speedToFlee
        });
    }
}
