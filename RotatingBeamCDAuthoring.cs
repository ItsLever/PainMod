using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Entities;

namespace PainMod;
[Il2CppImplements(typeof(IConvertGameObjectToEntity))]
public class RotatingBeamCDAuthoring : ModCDAuthoringBase
{
    
    public Il2CppValueField<float> speed;
    public Il2CppValueField<int> amt;
    public Il2CppValueField<ObjectID> id;
    public RotatingBeamCDAuthoring(IntPtr ptr) : base(ptr)
    {
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new RotatingBeamCD()
        {
            speed = speed,
            amount = amt,
            ObjectID = id
        });
    }
}