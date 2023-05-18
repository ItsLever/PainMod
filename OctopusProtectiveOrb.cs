using System;
using CoreLib;
using CoreLib.Components;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace PainMod;

public class OctopusProtectiveOrb : ModEntityMonoBehavior
{
    public Il2CppReferenceField<SpriteRenderer> SR;
    public Il2CppReferenceField<Sprite> crackStage0;
    public Il2CppReferenceField<Sprite> crackStage1;
    public Il2CppReferenceField<Sprite> crackStage2;
    public Il2CppReferenceField<Transform> pivot;
    private Il2CppValueField<int> lastCrackedStage;
    public OctopusProtectiveOrb(IntPtr ptr) : base(ptr) { }
    public override void OnOccupied()
    {
        this.CallBase<EntityMonoBehaviour>(nameof(OnOccupied));
        lastCrackedStage.Value = GetCrackLevel();
        Crack(lastCrackedStage, false);
    }
    public override void ManagedLateUpdate()
    {
        this.CallBase<EntityMonoBehaviour>(nameof(ManagedLateUpdate));
        if (!entityExist)
            return;
        int crackLevel = GetCrackLevel();
        if (crackLevel != lastCrackedStage.Value)
        {
            this.Crack(crackLevel, true);
            lastCrackedStage.Value = crackLevel;
        }
    }
    public override void OnDeath()
    {
        this.CallBase<Projectile>(nameof(OnDeath));
        PlayEffect();
    }

    private void Crack(int crackLevel, bool playEffects)
    {
        switch (crackLevel)
        {
            case 0:
                SR.Value.sprite = crackStage0.Value;
                break;
            case 1:
                SR.Value.sprite = crackStage1.Value;
                break;
            case 2:
                SR.Value.sprite = crackStage2.Value;
                break;
            default:
                SR.Value.sprite = crackStage0.Value;
                break;
        }
        if (playEffects)
        {
            this.PlayEffect();
        }
    }
    private void PlayEffect()
    {
        Vector3 position = particleOptions.particleSpawnLocations.ToArray()[0].position;
        Manager.effects.PlayPuff(PuffID.CrystalDebris, position, 10);
        AudioManager.Sfx(SfxID.wall, position, 1f, 1.5f, 0.1f, false, AudioManager.MixerGroupEnum.EFFECTS, false, true, false, 16f, 10f, false, true);
    }
    private int GetCrackLevel()
    {
        int currentHealth = this.currentHealth;
        //int maxHealth = GetMaxHealth();
        //if ((int)(currentHealth/maxHealth) <= 0.33)
        if(currentHealth <= 2)
            return 2;
        if (currentHealth <= 4)
            return 1;
        return 0;
    }
}