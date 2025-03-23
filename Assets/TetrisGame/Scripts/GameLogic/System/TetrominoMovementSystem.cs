using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.NetCode;
using UnityEngine;

namespace GameLogic
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            Entities
                .WithAll<Tetromino, GhostOwnerIsLocal>()
                .ForEach((ref LocalTransform transform, in PlayerInputData input) =>
                {
                    float move = input.Move.x;
                    if (math.abs(move) > 0f)
                    {
                        float newX = transform.Position.x + move * deltaTime * 5f; 
                        newX = math.clamp(newX, 0, TetrisGrid.Width - 1);
                        transform.Position.x = newX;
                    }

                    if (input.Rotate)
                    {
                        Debug.Log("Rotate input received");
                    }
                }).ScheduleParallel();
        }
    }
}