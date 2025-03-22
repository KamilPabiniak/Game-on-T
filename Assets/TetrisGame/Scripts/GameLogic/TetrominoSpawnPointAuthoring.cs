using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;

namespace GameLogic
{
    public class TetrominoSpawnPointAuthoring : MonoBehaviour
    {
        [Tooltip("0 – host, 1 – client")]
        public int PlayerId;
    }

    public struct TetrominoSpawnPoint : IComponentData
    {
        public int PlayerId;
        public float3 Position;
    }

    public class TetrominoSpawnPointBaker : Baker<TetrominoSpawnPointAuthoring>
    {
        public override void Bake(TetrominoSpawnPointAuthoring authoring)
        {
            AddComponent(new TetrominoSpawnPoint
            {
                PlayerId = authoring.PlayerId,
                Position = GetComponent<Transform>().position
            });
        }
    }
}