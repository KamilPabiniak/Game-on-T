using Unity.Entities;
using Unity.Mathematics;

namespace GameLogic
{
    public struct Tetromino : IComponentData
    {
    }

    public struct TetrominoOffset : IBufferElementData
    {
        public int2 Value;
    }
}