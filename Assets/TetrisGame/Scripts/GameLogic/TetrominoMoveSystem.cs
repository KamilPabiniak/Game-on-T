using GameLogic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[UpdateAfter(typeof(InputSystem))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class TetrominoMoveSystem : SystemBase
{
    protected override void OnCreate()
    {
        Debug.Log("TetrominoMoveSystem created");
    }

    protected override void OnUpdate()
    {
        Debug.Log("TetrominoMoveSystem updating");
        var query = GetEntityQuery(ComponentType.ReadWrite<LocalTransform>(), ComponentType.ReadOnly<FallingTetromino>(), ComponentType.ReadOnly<GhostOwnerIsLocal>());
        Debug.Log($"Entities with FallingTetromino and GhostOwnerIsLocal: {query.CalculateEntityCount()}");

        Entities
            .WithAll<FallingTetromino, GhostOwnerIsLocal>()
            .ForEach((ref LocalTransform transform, in PlayerInputData input) =>
            {
                float move = input.move.x;
                Debug.Log($"Processing input for Tetromino: {move}");

                if (math.abs(move) > 0f)
                {
                    float newX = transform.Position.x + move;
                    newX = math.clamp(newX, 0, TetrisGrid.Width - 1);
                    transform.Position.x = newX;
                    Debug.Log($"Tetromino moved to: {transform.Position.x}");
                }
            }).ScheduleParallel();
    }
}