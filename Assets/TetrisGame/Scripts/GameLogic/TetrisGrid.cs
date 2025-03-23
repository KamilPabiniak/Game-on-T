using Unity.Burst;
using Unity.Collections;

[BurstCompile]
public struct TetrisGrid
{
    public const int Width = 30;
    public const int Height = 30;

    private NativeArray<bool> _grid;

    public TetrisGrid(Allocator allocator)
    {
        _grid = new NativeArray<bool>(Width * Height, allocator);
    }

    [BurstCompile]
    public void Dispose()
    {
        if (_grid.IsCreated)
        {
            _grid.Dispose();
        }
    }

    [BurstCompile]
    public bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return true;
        }
        return _grid[y * Width + x];
    }

    [BurstCompile]
    public void MarkCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _grid[y * Width + x] = true;
        }
    }

    [BurstCompile]
    public void ClearCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _grid[y * Width + x] = false;
        }
    }
}