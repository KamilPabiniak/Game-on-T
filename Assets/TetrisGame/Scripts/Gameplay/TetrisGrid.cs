public static class TetrisGrid
{
    public static readonly int Width = 10;
    public static readonly int Height = 20;
    private static readonly bool[,] Grid = new bool[Width, Height];
    
    public static bool IsCellOccupied(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return true;
        return Grid[x, y];
    }
    
    public static void MarkCell(int x, int y)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
            Grid[x, y] = true;
    }
}