using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace GameLogic
{
    [GhostComponent(PrefabType = GhostPrefabType.All)]
    public struct Tetromino : IComponentData
    {
        [GhostField]
        public int PlayerId;
        
        [GhostField]
        public float3 RotationPivot;
        
        [GhostField]
        public float4 Color;
    }

    public struct TetrominoOffset : IBufferElementData
    {
        [GhostField]
        public float2 Value;
    }

    public struct MaterialProperty : IComponentData
    {
        public float4 Color;
    }
}