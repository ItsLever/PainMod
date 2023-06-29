using Il2CppInterop.Runtime.Attributes;
using Unity.Entities;

namespace PainMod;

[Il2CppImplements(typeof(IComponentData))]
public struct BirdBossReworkCD
{
    public int internalState;
    public int typeOfattack;
    public float radiusToBorder;
    public int amountOfShockBarriers;

}