using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;

namespace TMG.Survivors
{
    public struct PlayerTag : IComponentData { }

    public class PlayerAuthoring : MonoBehaviour
    {
        private class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<PlayerTag>(entity);
            }
        }
    }

    public partial class PlayerInputSystem : SystemBase
    {
        private SurvivorInput _input;

        protected override void OnCreate()
        {
            _input = new SurvivorInput();
            _input.Enable();
        }

        protected override void OnUpdate()
        {
            var currInput = (float2) _input.Player.Move.ReadValue<Vector2>();

            foreach (var direction in SystemAPI.Query<RefRW<CharacterMoveDirection>>().WithAll<PlayerTag>())
            {
                direction.ValueRW.value = currInput;
            }
        }
    }
}