using Unity.Burst;
using Unity.Entities;
using Unity.Rendering;

namespace GameLogic
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class TetrominoColorSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((ref URPMaterialPropertyBaseColor color, in MaterialProperty matProperty) =>
            {
                color.Value = matProperty.Color;
            }).ScheduleParallel();
        }
    }
}