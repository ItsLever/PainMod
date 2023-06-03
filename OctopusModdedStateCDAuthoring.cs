using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Entities;
using Unity.Mathematics;

namespace PainMod;
[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class OctopusModdedStateCDAuthoring : ModCDAuthoringBase
{
    public Il2CppValueField<float> hpToEnter;
    public Il2CppValueField<float> hpToEnter2;
    public Il2CppValueField<int> iteration;

    public OctopusModdedStateCDAuthoring(IntPtr ptr) : base(ptr) { }
    
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new OctopusModdedStateCD()
        {
            HpRatioToEnterState = hpToEnter,
            HpRatioToEnterState2 = hpToEnter2,
            iteration = iteration
        });
    }
}