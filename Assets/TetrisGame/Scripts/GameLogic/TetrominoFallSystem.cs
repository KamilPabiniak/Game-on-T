using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace GameLogic
{
    public struct FallSpeed : IComponentData
    {
        public float Value;
    }
    
    public struct FallingTetromino : IComponentData { }
    
    /// <summary>
    /// Updates the falling movement of tetromino pieces.
    /// </summary>
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoFallSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            Entities.WithAll<FallingTetromino>().ForEach((ref LocalTransform transform, in FallSpeed fallSpeed) =>
            {
                transform.Position.y -= fallSpeed.Value * deltaTime;
            }).ScheduleParallel();
        }
    }
}