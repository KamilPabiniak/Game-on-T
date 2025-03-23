using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace GameLogic
{
   
    public struct TetrominoSpawnPoint : IComponentData
    {
        public float3 Position;
    }
    
    public class TetrominoSpawnPointBaker : Baker<TetrominoSpawnPointAuthoring>
    {
        public override void Bake(TetrominoSpawnPointAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new TetrominoSpawnPoint
            {
                Position = authoring.transform.position
            });
        }
    }
}