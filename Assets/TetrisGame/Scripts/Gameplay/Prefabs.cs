using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class Prefabs : MonoBehaviour
{
    public List<GameObject> blocks;
}

public struct PrefabElement : IBufferElementData
{
    public Entity Value;
}

public class PrefabsBaker : Baker<Prefabs>
{
    public override void Bake(Prefabs authoring)
    {
        var entity = GetEntity(TransformUsageFlags.Dynamic);
        var buffer = AddBuffer<PrefabElement>(entity);
        
        if (authoring.blocks != null)
        {
            for (int i = 0; i < authoring.blocks.Count; i++)
            {
                if (authoring.blocks[i] != null)
                {
                    var entityPref = GetEntity(authoring.blocks[i], TransformUsageFlags.Dynamic);
                    buffer.Add(new PrefabElement { Value = entityPref });
                }
            }
        }
    }
}