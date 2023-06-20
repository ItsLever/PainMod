using System;
using System.Runtime.InteropServices;
using CoreLib;
using CoreLib.Components;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace PainMod;

public class CustomBirdBossBarrier : ModEntityMonoBehavior
{
    public Il2CppReferenceField<SpriteRenderer> spriteRenderer;

    public Il2CppReferenceField<List<Sprite>> frames;
    private GCHandle framesHandle;


    public CustomBirdBossBarrier(IntPtr ptr) : base(ptr) { }

    public override bool Allocate()
    {
        bool shouldAllocate = base.Allocate();
        if (shouldAllocate)
        { framesHandle = GCHandle.Alloc(frames.Value);
        }
        return shouldAllocate;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        framesHandle.Free();
    }

    public override void ManagedLateUpdate()
    {
        this.CallBase<EntityMonoBehaviour>(nameof(ManagedLateUpdate));

        if (entityExist)
        {
            int frame = (int)(Time.time * 15) % frames.Value.Count;
            spriteRenderer.Value.sprite = frames.Value._items[frame];
        }
    }
}