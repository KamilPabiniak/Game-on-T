using System.Collections.Generic;
using Common.Scripts;
using TetrisGame.Scripts.UI;
using UnityEngine;

namespace TetrisGame.GameLogic
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance;

        [Header("Board Settings")]
        public int boardWidth;
        public int boardHeight;

        [Tooltip("Number of top rows considered as game over zone")]
        public int gameOverZoneHeight = 2;

        [Tooltip("Points awarded per cleared row.")]
        public int scorePerLine = 100;
        
        private readonly Dictionary<Vector2Int, GameObject> _grid = new();

        private void Awake()
        {
            Instance = this;
        }
        
        public void PlaceTetromino(GameObject tetromino)
        {
            Vector3 boardOrigin = transform.position;
            List<Transform> blocks = new List<Transform>();

            foreach (Transform block in tetromino.transform)
            {
                blocks.Add(block);
            }
            
            foreach (Transform block in blocks)
            {
                Vector3 localPos = block.position - boardOrigin;
                int gridX = Mathf.RoundToInt(localPos.x);
                int gridY = Mathf.RoundToInt(localPos.y);
                Vector2Int gridPos = new Vector2Int(gridX, gridY);

                if (gridX >= 0 && gridX < boardWidth && gridY >= 0 && gridY < boardHeight && !_grid.ContainsKey(gridPos)) continue;
                Destroy(tetromino);
                return;
            }
            
            foreach (Transform block in blocks)
            {
                Vector3 localPos = block.position - boardOrigin;
                int gridX = Mathf.RoundToInt(localPos.x);
                int gridY = Mathf.RoundToInt(localPos.y);
                Vector2Int gridPos = new Vector2Int(gridX, gridY);
                
                block.position = boardOrigin + new Vector3(gridX + 0.5f, gridY + 0.5f, 0);
                block.parent = null;
                GameObject o = block.gameObject;
                o.layer = LayerMask.NameToLayer("Snaped");
        
                _grid[gridPos] = o;
            }
            ClearFullRows();
            GameManager.Instance.GlobalFallSpeed += 0.06f;
            Destroy(tetromino);
        }
        
        private void ClearFullRows()
        {
            List<int> fullRows = new List<int>();
            
            for (int y = 0; y < boardHeight; y++)
            {
                int count = 0;
                for (int x = 0; x < boardWidth; x++)
                {
                    if (_grid.ContainsKey(new Vector2Int(x, y)))
                    {
                        count++;
                    }
                }
                if (count >= boardWidth)
                {
                    fullRows.Add(y);
                }
            }
            
            foreach (int y in fullRows)
            {
                ClearRow(y);
                TetrisUIManager.Instance.UpdateScore(GameManager.Instance.Score += scorePerLine);
            }
            
            foreach (int y in fullRows)
            {
                ShiftRowsDown(y + 1);
            }
        }
        
        private void ClearRow(int y)
        {
            for (int x = 0; x < boardWidth; x++)
            {
                Vector2Int pos = new Vector2Int(x, y);
                if (_grid.ContainsKey(pos))
                {
                    int blockId = _grid[pos].GetInstanceID();
                    VisualEvetns.RaiseBlockRemoved(blockId);
                    Destroy(_grid[pos]);
                    _grid.Remove(pos);
                }
            }
        }
        
        public bool CanSpawnTetromino(Vector3 spawnPosition, Vector3[] shapes)
        {
            Vector3 boardOrigin = transform.position;
            foreach (var shape in shapes)
            {
                Vector3 worldPos = spawnPosition + shape;
                Vector3 localPos = worldPos - boardOrigin;
                
                int gridX = Mathf.RoundToInt(localPos.x);
                int gridY = Mathf.FloorToInt(localPos.y);
        
                if(gridX < 0 || gridX >= boardWidth || gridY < 0 || gridY >= boardHeight)
                    return false;
            
                if(IsPositionOccupied(new Vector2Int(gridX, gridY)))
                    return false;
            }
            return true;
        }
        
        private void ShiftRowsDown(int startY)
        {
            for (int y = startY; y < boardHeight; y++)
            {
                for (int x = 0; x < boardWidth; x++)
                {
                    Vector2Int pos = new Vector2Int(x, y);
                    if (_grid.ContainsKey(pos))
                    {
                        GameObject block = _grid[pos];
                        block.transform.position += Vector3.down;
                        _grid.Remove(pos);
                        Vector2Int newPos = new Vector2Int(x, y - 1);
                        _grid[newPos] = block;
                    }
                }
            }
        }
        
        public bool IsPositionOccupied(Vector2Int gridPos)
        {
            return _grid.ContainsKey(gridPos);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Vector3 startPos = transform.position;

            for (int y = 0; y <= boardHeight; y++)
            {
                Gizmos.DrawLine(
                    startPos + new Vector3(0, y, 0),
                    startPos + new Vector3(boardWidth, y, 0)
                );
            }

            for (int x = 0; x <= boardWidth; x++)
            {
                Gizmos.DrawLine(
                    startPos + new Vector3(x, 0, 0),
                    startPos + new Vector3(x, boardHeight, 0)
                );
            }
            
            Gizmos.color = new Color(1, 0, 0, 0.7f);
            foreach (var kvp in _grid)
            {
                Vector3 center = transform.position + new Vector3(kvp.Key.x + 0.5f, kvp.Key.y + 0.5f, 0);
                Gizmos.DrawCube(center, Vector3.one * 0.9f);
            }
            
            Gizmos.color = new Color(1, 1, 0, 1f);
            float zoneCenterY = boardHeight - (gameOverZoneHeight / 2f);
            Vector3 zoneCenter = new Vector3(boardWidth / 2f, zoneCenterY, 0);
            Vector3 zoneSize = new Vector3(boardWidth, gameOverZoneHeight, 0);
            Gizmos.DrawCube(zoneCenter, zoneSize);
        }
    }
}
