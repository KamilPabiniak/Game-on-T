using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
     public partial class TetrominoSnapSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities
                .WithAll<FallingTetromino>()
                .WithoutBurst()
                .ForEach((Entity entity, ref LocalTransform transform, in DynamicBuffer<TetrominoBlock> blockBuffer) =>
                {
                    bool shouldSnap = false;
                    for (int i = 0; i < blockBuffer.Length; i++)
                    {
                        TetrominoBlock block = blockBuffer[i];
                        int gridX = Mathf.RoundToInt(transform.Position.x + block.Offset.x);
                        int gridY = Mathf.RoundToInt(transform.Position.y + block.Offset.y);
                        if (gridY <= 0 || TetrisGrid.IsCellOccupied(gridX, gridY - 1))
                        {
                            shouldSnap = true;
                            break;
                        }
                    }

                    if (shouldSnap)
                    {
                        transform.Position.x = Mathf.Round(transform.Position.x);
                        transform.Position.y = Mathf.Round(transform.Position.y);
                        
                        for (int i = 0; i < blockBuffer.Length; i++)
                        {
                            TetrominoBlock block = blockBuffer[i];
                            int cellX = Mathf.RoundToInt(transform.Position.x + block.Offset.x);
                            int cellY = Mathf.RoundToInt(transform.Position.y + block.Offset.y);
                            TetrisGrid.MarkCell(cellX, cellY);
                        }
                        
                        ecb.RemoveComponent<FallingTetromino>(entity);
                        ecb.AddComponent(entity, new TetrominoPlaced());
                        Debug.Log($"Tetromino snapped and placed at grid position ({transform.Position.x}, {transform.Position.y})");
                    }
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}