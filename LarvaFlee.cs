using CoreLib.Components;
using CoreLib.Submodules.ModComponent;
using CoreLib.Submodules.ModSystem;
using Il2CppInterop.Runtime.Attributes;
using Il2CppInterop.Runtime.InteropTypes.Fields;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace PainMod
{
    internal class LarvaFlee
    {
        public class LarvaFleeStateRequester : IStateRequester
        {
            // Use a unique id to identify your custom state
            public const string FLEE_STATE_ID = "MyMod:FleeState";
            public static StateID fleeState = SystemModule.GetModStateId(FLEE_STATE_ID);

            // This class will help us get our component
            private ModComponentDataFromEntity<LarvaFleeStateCD> larvaStateGroup;

            // In case of multiple requesters changing same entity state one with higher priority will get executed first
            public int priority => SystemModule.NORMAL_PRIORITY;

            //int IStateRequester.priority => throw new NotImplementedException();

            // Use this method to intialize your state
            public void OnCreate(World world2)
            {
                larvaStateGroup = new ModComponentDataFromEntity<LarvaFleeStateCD>(world2.EntityManager);
            }

            // The method will be called for each entity. 
            // You need to determine if your entity should have the state
            // This is usually done by checking if your component exists
            public bool OnUpdate(Entity entity, EntityCommandBuffer ecb, ref StateRequestData data, ref StateRequestContainers containers, ref StateInfoCD stateInfo)
            {
                // Does this entity have our custom component?
                if (!larvaStateGroup.HasComponent(entity)) return false;

                // Get needed data
                HealthCD healthCd = containers._healthGroup[entity];
                LarvaFleeStateCD larvaFleeStateCd = larvaStateGroup[entity];
                float healthPercent = healthCd.health / (float)healthCd.maxHealth;

                // If the entity has too low HP enter flee state
                if (stateInfo.currentState == StateID.Chase &&
                    healthPercent < larvaFleeStateCd.fleeAtHealthRatio)
                {
                    Plugin.logger.LogInfo("Enter Flee State");
                    stateInfo.newState = fleeState;
                    // By returning true here we signal that the 'stateInfo' field has changed
                    return true;
                }

                // The entity is fleeing 
                if (stateInfo.currentState == fleeState)
                {
                    stateInfo.newState = fleeState;
                    // Determine if it should keep fleeing
                    if (healthPercent > larvaFleeStateCd.leaveFleeAtHealthRatio)
                    {
                        stateInfo.newState = StateID.RandomWalking;
                    }
                    // By returning true here we signal that the 'stateInfo' field has changed
                    return true;
                }

                // Nothing changed
                return false;
            }

            void IStateRequester.OnCreate(World world2)
            {
                larvaStateGroup = new ModComponentDataFromEntity<LarvaFleeStateCD>(world2.EntityManager);
            }

            bool IStateRequester.ShouldUpdate(Entity entity, ref StateRequestData data, ref StateRequestContainers containers)
            {
                return larvaStateGroup.HasComponent(entity);
            }
        }

        public class LarvaFleeStateSystem : MonoBehaviour, IPseudoServerSystem
        {
            private World serverWorld2;
            private EntityQuery entityQuery;

            public void OnServerStarted(World world2)
            {
                // Prepare our state. This entity query will return only entities that match our components
                serverWorld2 = world2;
                entityQuery = serverWorld2.EntityManager.CreateEntityQuery(
                    ComponentModule.ReadOnly<LarvaFleeStateCD>(),
                    ComponentModule.ReadWrite<Translation>(),
                    ComponentModule.ReadOnly<LastAttackerCD>(),
                    ComponentModule.ReadOnly<StateInfoCD>());
            }

            public void OnServerStopped()
            {
                // The game is about to stop, clear our reference to World
                serverWorld2 = null;
            }
            
            private void FixedUpdate()
            {
                if (serverWorld2 == null) return;

                // Execute our query and itterate the entities
                var entities = entityQuery.ToEntityArray(Allocator.Temp);

                foreach (Entity entity in entities)
                {
                    // Get StateInfoCD component, and see if entity is in our state
                    StateInfoCD stateInfo = serverWorld2.EntityManager.GetModComponentData<StateInfoCD>(entity);
                    if (stateInfo.currentState == LarvaFleeStateRequester.fleeState)
                    {
                        // Fetch needed components
                        LarvaFleeStateCD fleeStateCd = serverWorld2.EntityManager.GetModComponentData<LarvaFleeStateCD>(entity);
                        Translation translation = serverWorld2.EntityManager.GetModComponentData<Translation>(entity);

                        // Find last attacker entity
                        LastAttackerCD lastAttackerCd = serverWorld2.EntityManager.GetModComponentData<LastAttackerCD>(entity);
                        Entity attackerEntity = lastAttackerCd.Value;

                        // If attacker exists, do our thing
                        if (serverWorld2.EntityManager.Exists(attackerEntity))
                        {
                            Translation attackerTranslation = serverWorld2.EntityManager.GetModComponentData<Translation>(attackerEntity);

                            Plugin.logger.LogInfo("This "+ entity +" entity is Fleeing!");

                            // This will result in a very basic straight line movement behavior
                            float3 dir = math.normalize(translation.Value - attackerTranslation.Value);
                            translation.Value += dir * fleeStateCd.fleeSpeed * Time.fixedDeltaTime;

                            serverWorld2.EntityManager.SetModComponentData(entity, translation);
                        }
                    }
                }
            }
        }
    }
}
