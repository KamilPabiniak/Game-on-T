using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    public struct FallingTetromino : IComponentData { }

    public struct FallSpeed : IComponentData
    {
        public float Value;
    }
    
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class FallingTetrominoSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;

            Entities
                .WithAll<FallingTetromino>()
                .ForEach((ref LocalTransform transform, in FallSpeed speed) =>
                {
                    transform.Position.y -= speed.Value * deltaTime;
                }).ScheduleParallel();
        }
    }
}