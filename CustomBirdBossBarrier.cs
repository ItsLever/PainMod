using System;
using System.Runtime.InteropServices;
using CoreLib;
using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Il2CppSystem.Collections.Generic;
using UnityEngine;

namespace PainMod;

public class CustomBirdBossBarrier : ModEntityMonoBehavior
{
    public Il2CppReferenceField<SpriteRenderer> spriteRenderer;

    public Il2CppReferenceField<List<Sprite>> frames;
    private GCHandle framesHandle;
    private Il2CppValueField<AttackContinuouslyCD> attackContinuously;
    private Il2CppValueField<float> lastTriggerTime;
    private Il2CppValueField<float>timeBetweenAttacks;
    private Il2CppReferenceField<PoolableAudioSource> audioLoop;
    private GCHandle audioLoopHandle;
    private Il2CppValueField<int> damage;


    public CustomBirdBossBarrier(IntPtr ptr) : base(ptr) { }
    public override void OnOccupied()
    {
        //this.audioLoop.Value = AudioManager.SfxFollowTransform(SfxID.Bird_Boss_Energy_Pillars_Attack, base.transform, 0.4f, 1f, 0f, false, AudioManager.MixerGroupEnum.EFFECTS, false, true, true, 6f, 10f, false, true);
        this.damage.Value = EntityUtility.GetComponentData<AttackContinuouslyCD>(base.entity, base.world).damage;
        this.CallBase<EntityMonoBehaviour>(nameof(OnOccupied));
    }
    public override bool Allocate()
    {
        bool shouldAllocate = base.Allocate();
        if (shouldAllocate)
        { 
            framesHandle = GCHandle.Alloc(frames.Value);
            audioLoopHandle = GCHandle.Alloc(audioLoop.Value);
        }
        return shouldAllocate;
    }


    public override void OnDestroy()
    {
        base.OnDestroy();

        framesHandle.Free();
        audioLoopHandle.Free();
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

    public override void OnPlayerTriggerEnter(PlayerController pc)
    {
        if (EntityUtility.GetComponentData<FactionCD>(base.entity, base.world).originalFaction == FactionID.Player)
        {
            this.CallBase<EntityMonoBehaviour>(nameof(OnPlayerTriggerEnter));
            return;
        }
        if (pc.isLocal)
        {
            base.world.GetExistingSystem<AttackPlayerClientSystem>().RegisterPlayerHit(base.entity, pc, pc.WorldPosition, damage, 0, 0f, 0f, 0, false, true, false);
        }
        else
        {
            base.world.GetExistingSystem<AttackPlayerClientSystem>().CheckIfHit(base.entity, pc, this.damage, false, true);
        }
        //pc.Kill();
        this.CallBase<EntityMonoBehaviour>(nameof(OnPlayerTriggerEnter));
    }
}