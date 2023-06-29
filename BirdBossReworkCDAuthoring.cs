using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Entities;

namespace PainMod;

[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class BirdBossReworkCDAuthoring : ModCDAuthoringBase
{
    public BirdBossReworkCDAuthoring(IntPtr ptr) : base(ptr) { }
    public Il2CppValueField<int> iState;
    public Il2CppValueField<int> type;
    public Il2CppValueField<float> radius;
    public Il2CppValueField<int> amount;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new BirdBossReworkCD()
        {
            internalState = iState,
            typeOfattack = type,
            radiusToBorder = radius,
            amountOfShockBarriers = amount
        });
    }
}