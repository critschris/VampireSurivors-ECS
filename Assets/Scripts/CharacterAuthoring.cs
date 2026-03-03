using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Burst;
using UnityEngine;
using Unity.Rendering;

namespace TMG.Survivors
{
    //Initialization method 1: Disable flag component
    public struct InitializeCharacterFlag : IComponentData, IEnableableComponent { }

    public struct CharacterMoveDirection : IComponentData
    {
        public float2 value;
    }

    public struct CharacterMoveSpeed : IComponentData
    {
        public float value;
    }

    [MaterialProperty("_FacingDirection")]
    public struct FacingDirectionOverride : IComponentData
    {
        public float value;
    }

    //Baking mono behavior component into entity component
    public class CharacterAuthoring : MonoBehaviour
    {

        public float moveSpeed;
        private class Baker : Baker<CharacterAuthoring>
        {
            public override void Bake(CharacterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<CharacterMoveDirection>(entity);
                AddComponent<InitializeCharacterFlag>(entity);
                AddComponent(entity, new CharacterMoveSpeed
                {
                    value = authoring.moveSpeed
                });
                AddComponent(entity, new FacingDirectionOverride
                {
                    value = 1
                });
            }
        }
    }

    
    //System to initialize character movement settings (to not rotate here)
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CharacterInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //Query to find entities with PhysicsMass and InitializeCharacterFlag
            foreach (var (mass, flag) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<InitializeCharacterFlag>>())
            {
                mass.ValueRW.InverseInertia = float3.zero;

                //Disabled flag
                flag.ValueRW = false;
            }
        }
    }

    //System to apply movement to entities with PhysicsVelocity, CharacterMoveDirection and CharacterMoveSpeed
    public partial struct CharacterMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            //Query to find entities with PhysicsVelocity, CharacterMoveDirection and CharacterMoveSpeed
            foreach (var (velocity, facing, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>, RefRW<FacingDirectionOverride>,CharacterMoveDirection, CharacterMoveSpeed>())
            {
                //moving in movestep2d direction
                var moveStep2d = direction.value * speed.value;
                velocity.ValueRW.Linear = new float3(moveStep2d,0f);

                //Updating facing direction whenever movement over threshold was passed
                if (math.abs(moveStep2d.x) > 0.15f)
                {
                    facing.ValueRW.value = math.sign(moveStep2d.x);
                }

            }
        }
    }

    //System to update "GlobalTime" in animation shader
    public partial struct GlobalTimeUpdateSystem : ISystem
    {
        private static int _GlobalTimeShaderPropertyID;

        public void OnCreate(ref SystemState state)
        {
            _GlobalTimeShaderPropertyID = Shader.PropertyToID("_GlobalTime");
        }

        public void OnUpdate(ref SystemState state)
        {
            Shader.SetGlobalFloat(_GlobalTimeShaderPropertyID, (float) SystemAPI.Time.ElapsedTime);
        }
    }
}


