using Unity.Entities;
using UnityEngine;

public class PrefabSingleton : MonoBehaviour
{
    public GameObject block;
}

public struct PrefabElement : IComponentData
{
    public Entity Value;
}

public class PrefabSingletonBaker : Baker<PrefabSingleton>
{
    public override void Bake(PrefabSingleton authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        AddComponent(entity, new PrefabElement { Value = GetEntity(authoring.block, TransformUsageFlags.Dynamic) });
    }
}