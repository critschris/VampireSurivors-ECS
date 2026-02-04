using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

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
    private class Baker : Baker<CharacterAuthoring>
    {
        public override void Bake(CharacterAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);

            AddComponent<CharacterMoveDirection>(entity);
            AddComponent(entity, new CharacterMoveSpeed
            {
                value = 5
            });
        }
    }
}
