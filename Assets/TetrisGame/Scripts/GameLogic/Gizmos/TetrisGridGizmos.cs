using UnityEngine;

public class TetrisGridGizmos : MonoBehaviour
{
    private float cellSize = 1f;
    public Color gridColor = Color.gray;

    private void OnDrawGizmos()
    {
        Gizmos.color = gridColor;
        for (int x = 0; x <= TetrisGrid.Width; x++)
        {
            Vector3 start = new Vector3(x * cellSize, 0f, 0f);
            Vector3 end = new Vector3(x * cellSize, TetrisGrid.Height * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }
        for (int y = 0; y <= TetrisGrid.Height; y++)
        {
            Vector3 start = new Vector3(0f, y * cellSize, 0f);
            Vector3 end = new Vector3(TetrisGrid.Width * cellSize, y * cellSize, 0f);
            Gizmos.DrawLine(start, end);
        }
    }
}