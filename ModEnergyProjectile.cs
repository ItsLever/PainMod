using System;
using CoreLib;
using CoreLib.Components;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using UnityEngine;

namespace PainMod
{
  public class ModEnergyProjectile : ModProjectile
{
    public Il2CppReferenceField<Transform> directionTransform;
    public Il2CppReferenceField<ParticleSystem> projectileFx;
    public Il2CppReferenceField<ParticleSystem> fireballSmoke;
    public Il2CppReferenceField<ParticleSystem> fireballFireTrail;
    public Il2CppReferenceField<ParticleSystem> hit;
    public Il2CppReferenceField<PugLight> fireLight;

    public ModEnergyProjectile(IntPtr ptr) : base(ptr) { }

    public override void OnOccupied()
    {
        Plugin.logger.LogInfo("Projectile is On Occupied, check for crashes please!");
        //this.CallBase<Projectile>(nameof(OnOccupied));
        int health = currentHealth;
        directionTransform.Value.gameObject.SetActive(health > 0);
        if (health <= 0) return;

        AudioManager.Sfx(SfxID.fireball, transform.position, 0.8f, 1, 0.1f);
        AudioManager.Sfx(SfxID.anicentDevicePowerUp, transform.position, 0.6f, 0.7f, 0.1f);
        ProjectileCD projectileCd = EntityUtility.GetComponentData<ProjectileCD>(entity, world);

        Vector3 dir = projectileCd.direction * 0.3f;
        Vector3 renderPos = ToRenderFromWorld(WorldPosition);
        Vector3 puffPos = renderPos + directionTransform.Value.localPosition + dir;
        
        Manager.effects.PlayPuff(PuffID.SmallEnergyExplosion, puffPos);

        dir = directionTransform.Value.position + (Vector3)projectileCd.direction;
        directionTransform.Value.transform.LookAt(dir, Vector3.up);
        
        projectileFx.Value.Play();
        if (fireballSmoke.Value != null)
            fireballSmoke.Value.Play();
        if (fireballFireTrail.Value != null)
            fireballFireTrail.Value.Play();
        fireLight.Value.gameObject.SetActive(true);
    }

    public override void OnDeath()
    {
        this.CallBase<Projectile>(nameof(OnDeath));
        
        if (projectileFx.Value != null && hit.Value != null)
        {
            projectileFx.Value.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            if (fireballSmoke.Value != null)
                fireballSmoke.Value.Stop();
            if (fireballFireTrail.Value != null)
                fireballFireTrail.Value.Stop();
            hit.Value.Play();
        }
        fireLight.Value.gameObject.SetActive(false);
        SpawnFadeOutLight(fireLight.Value.lightToOptimize);
    }
}
}