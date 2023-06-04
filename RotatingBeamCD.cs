using Il2CppInterop.Runtime.Attributes;
using Unity.Entities;

namespace PainMod;
[Il2CppImplements(typeof(IComponentData))]
public struct RotatingBeamCD
{
    public ObjectID ObjectID;
    public float speed;
    public int amount;
}