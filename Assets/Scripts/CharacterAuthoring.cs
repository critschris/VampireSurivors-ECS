using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;

namespace TMG.Survivors
{
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
                AddComponent(entity, new CharacterMoveSpeed
                {
                    value = authoring.moveSpeed
                });
            }
        }
    }

    public partial struct CharacterMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;

            foreach (var (velocity, direction, speed) in SystemAPI.Query<RefRW<PhysicsVelocity>,CharacterMoveDirection, CharacterMoveSpeed>())
            {
                var moveStep2d = direction.value * speed.value;
                velocity.ValueRW.Linear = new float3(moveStep2d,0f);
            }
        }
    }
}


