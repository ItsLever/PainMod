using System;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Unity.Entities;

namespace PainMod;

public class DoesCircleCDAuthoring : ModCDAuthoringBase
{
    public DoesCircleCDAuthoring(IntPtr ptr) : base(ptr)
    {
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddModComponentData(entity, new DoesCircleCD());
    }
}