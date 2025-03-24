using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoSnapSystem : SystemBase
    {
        private TetrisGrid _grid;

        protected override void OnCreate()
        {
            _grid = new TetrisGrid(Allocator.Persistent);
        }

        protected override void OnDestroy()
        {
            _grid.Dispose();
        }

        protected override void OnUpdate()
        {
            var grid = _grid;
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            Entities
                .WithAll<Tetromino>()
                .ForEach((Entity entity, ref LocalTransform transform, in DynamicBuffer<TetrominoOffset> offsets) =>
                {
                    bool shouldSnap = false;
                    foreach (var offset in offsets)
                    {
                        int gridX = Mathf.RoundToInt(transform.Position.x + offset.Value.x);
                        int gridY = Mathf.RoundToInt(transform.Position.y + offset.Value.y);

                        // Jeśli którykolwiek blok dotyka podłogi lub istnieje blok poniżej – snapuj
                        if (gridY <= 0 || grid.IsCellOccupied(gridX, gridY - 1))
                        {
                            shouldSnap = true;
                            break;
                        }
                    }

                    if (shouldSnap)
                    {
                        // Wyrównanie pozycji do siatki
                        transform.Position = new float3(Mathf.Round(transform.Position.x), Mathf.Round(transform.Position.y), transform.Position.z);
                        foreach (var offset in offsets)
                        {
                            int gridX = Mathf.RoundToInt(transform.Position.x + offset.Value.x);
                            int gridY = Mathf.RoundToInt(transform.Position.y + offset.Value.y);
                            grid.MarkCell(gridX, gridY);
                        }
                        ecb.RemoveComponent<Tetromino>(entity);
                        Debug.Log("Tetromino snapped to grid");
                    }
                }).Run();

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}
