using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Burst;
using UnityEngine;

namespace TMG.Survivors
{
    public struct InitializeCharacterFlag : IComponentData, IEnableableComponent { }

    public struct CharacterMoveDirection : IComponentData
    {
        public float2 value;
    }

    public struct CharacterMoveSpeed : IComponentData
    {
        public float value;
    }

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
            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CharacterInitializationSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {

            foreach (var (mass, flag) in SystemAPI.Query<RefRW<PhysicsMass>, EnabledRefRW<InitializeCharacterFlag>>())
            {
                mass.ValueRW.InverseInertia = float3.zero;
                flag.ValueRW = false;
            }
        }
    }

    public partial struct CharacterMoveSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {        
            foreach (var (velocity, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>,CharacterMoveDirection, CharacterMoveSpeed>())
            {
                var moveStep2d = direction.value * speed.value;
                velocity.ValueRW.Linear = new float3(moveStep2d,0f);
            }
        }
    }
}


