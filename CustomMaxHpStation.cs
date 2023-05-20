using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using CoreLib;
using CoreLib.Components;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using Unity.Mathematics;
using UnityEngine;
namespace PainMod
{

    public class CustomMaxHpStation : ModEntityMonoBehavior
    {
        public Il2CppValueField<ConditionID> eff;
        public Il2CppValueField<int> value;
        public Il2CppValueField<int> duration;
        public CustomMaxHpStation(IntPtr ptr) : base(ptr) { }
        public override void OnOccupied()
        {
            this.CallBase<EntityMonoBehaviour>(nameof(OnOccupied));
        }

        public void OnUse()
        {
            AudioManager.Sfx(SfxID.eating, transform.position, 0.8f, 1, 0.1f);
            Manager._instance.player.playerCommandSystem.AddOrRefreshCondition(Manager._instance.player.entity, eff.Value, value, duration);
        }
    }
}