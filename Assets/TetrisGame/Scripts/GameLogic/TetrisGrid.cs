public static class TetrisGrid
{
    public const int Width = 30;
    public const int Height = 20;
    private static bool[] grid = new bool[Width * Height];

    public static bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return false;
        return grid[GetIndex(x, y)];
    }

    public static void MarkCell(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;
        grid[GetIndex(x, y)] = true;
    }

    private static int GetIndex(int x, int y)
    {
        return x + y * Width;
    }
}