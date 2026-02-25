using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TMG.Survivors
{
    public struct PlayerTag : IComponentData { }

    public struct CameraTarget : IComponentData 
    {
        public UnityObjectRef<Transform> CameraTransform;
    }

    public struct InitializeCameraTargetTag : IComponentData { }

    public class PlayerAuthoring : MonoBehaviour
    {
        private class Baker : Baker<PlayerAuthoring>
        {
            public override void Bake(PlayerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddComponent<PlayerTag>(entity);
                AddComponent<InitializeCameraTargetTag>(entity);
            }
        }
    }

    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct CameraInitializationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<InitializeCameraTargetTag>();
        }

        public void OnUpdate(ref SystemState state)
        {
            if (CameraTargetSingleton.instance == null) return;
            var cameraTargetTransform = CameraTargetSingleton.instance.transform;

            var ecb = new EntityCommandBuffer(state.WorldUpdateAllocator);
            foreach (var (cameraTarget, entity) in SystemAPI.Query<RefRW<CameraTarget>>().WithAll<InitializeCameraTargetTag, PlayerTag>().WithEntityAccess())
            {
                cameraTarget.ValueRW.CameraTransform = cameraTargetTransform;
                ecb.RemoveComponent<InitializeCameraTargetTag>(entity);
            }

            ecb.Playback(state.EntityManager);
        }
    }

    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct CameraMoveSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (transform, cameraTarget) in SystemAPI.Query<LocalToWorld,CameraTarget>().WithAll<PlayerTag>().WithNone<InitializeCameraTargetTag>())
            {
                cameraTarget.CameraTransform.Value.position = transform.Position;
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