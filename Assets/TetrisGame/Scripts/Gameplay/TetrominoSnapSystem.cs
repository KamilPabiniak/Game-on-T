using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    public partial class TetrominoSnapSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Unity.Collections.Allocator.Temp);
            
            Entities
                .WithAll<FallingTetromino>()
                .WithoutBurst()
                .ForEach((Entity entity, ref LocalTransform transform, in DynamicBuffer<TetrominoBlock> blocks) =>
                {
                    bool shouldSnap = false;
                    for (int i = 0; i < blocks.Length; i++)
                    {
                        var block = blocks[i];
                        int cellX = Mathf.RoundToInt(transform.Position.x + block.Offset.x);
                        int cellY = Mathf.RoundToInt(transform.Position.y + block.Offset.y);
                        int belowY = cellY - 1;
                        if (belowY < 0 || TetrisGrid.IsCellOccupied(cellX, belowY))
                        {
                            shouldSnap = true;
                            break;
                        }
                    }

                    if (shouldSnap)
                    {
                        transform.Position.x = Mathf.Round(transform.Position.x);
                        transform.Position.y = Mathf.Round(transform.Position.y);
                        
                        for (int i = 0; i < blocks.Length; i++)
                        {
                            var block = blocks[i];
                            int cellX = Mathf.RoundToInt(transform.Position.x + block.Offset.x);
                            int cellY = Mathf.RoundToInt(transform.Position.y + block.Offset.y);
                            TetrisGrid.MarkCell(cellX, cellY);
                        }
                        
                        ecb.RemoveComponent<FallingTetromino>(entity);
                        ecb.AddComponent(entity, new TetrominoPlaced());

                        Debug.Log("Snaped and set");
                    }
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}