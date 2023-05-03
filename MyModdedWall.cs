using System.Runtime.InteropServices;
using CoreLib;
using CoreLib.Components;
using CoreLib.Submodules.ModEntity;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem;
using Il2CppSystem.Collections.Generic;
using UnityEngine;
using IntPtr = System.IntPtr;

namespace PainMod;
public class MyModdedWall : ModEntityMonoBehavior
{
    public Il2CppReferenceField<SpriteRenderer> mainRenderer;
    private GCHandle mainSpritesHandle;
    public Il2CppReferenceField<List<Sprite>> mainSprites;

    public MyModdedWall(IntPtr ptr) : base(ptr)
    {
    }

    public override bool Allocate()
    {
        bool shouldAlloc = base.Allocate();
        if (shouldAlloc)
        {
            mainSpritesHandle = GCHandle.Alloc(mainSprites.Value);
        }

        return shouldAlloc;
    }
    public override void OnDestroy()
    {
        base.OnDestroy();
        mainSpritesHandle.Free();
    }
    public override void ManagedLateUpdate()
    {
        this.CallBase<EntityMonoBehaviour>(nameof(ManagedLateUpdate));

        if (entityExist)
        {
            Plugin.logger.LogInfo("Main update");
        }
    }
}