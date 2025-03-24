using GameLogic;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

public class TetrominoAuthoring : MonoBehaviour
{
    class Baker : Baker<TetrominoAuthoring>
    {
        public override void Bake(TetrominoAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent<Tetromino>(entity);
            AddBuffer<TetrominoOffset>(entity);
            AddComponent<GhostOwner>(entity);
            AddComponent<MaterialProperty>(entity);
            AddComponent<FallingTetromino>(entity);
            AddComponent<FallSpeed>(entity);
        }
    }
}