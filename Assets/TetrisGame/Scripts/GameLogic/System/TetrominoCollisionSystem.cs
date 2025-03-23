using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoCollisionSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities
                .WithAll<Tetromino>()
                .ForEach((ref LocalTransform transform) =>
                {
                    if (transform.Position.y <= 0)
                    {
                       //Gdzie jest kurwa logika snapowania???
                    }
                }).ScheduleParallel();
        }
    }
}