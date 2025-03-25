using Common.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TetrisGame.Scripts.UI
{
    /// <summary>
    /// Manages the minimalistic UI for the Tetris game
    /// Displays the next tetromino preview for both players (using 3x3 grids), current score, and player names
    /// </summary>
    public class TetrisUIManager : MonoBehaviour
    {
        public static TetrisUIManager Instance { get; private set; }

        [Header("UI Elements - Score and Names")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI bluePlayerNameText;
        [SerializeField] private TextMeshProUGUI redPlayerNameText;
        [SerializeField] private GameObject gameOverScreen;
        

        [Header("UI Elements - Next Tetromino Preview")]
        [Tooltip("3x3 grid of Image elements for Blue player's next tetromino preview (ordered row-major)")]
        [SerializeField] private Image[] blueNextTetrominoCells;
        [Tooltip("3x3  grid of Image elements for Red player's next tetromino preview")]
        [SerializeField] private Image[] redNextTetrominoCells;

        [Header("Preview Colors")]
        [SerializeField] private Color blueTetrominoColor = Color.blue;
        [SerializeField] private Color redTetrominoColor = Color.red;
        [SerializeField] private Color emptyCellColor = new Color(0, 0, 0, 0);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }

            UpdateScore(0);
        }

        private void OnEnable()
        {
            GameEvent.OnGameOver += ShowGameOver;
        }

        private void OnDisable()
        {
            GameEvent.OnGameOver -= ShowGameOver;
        }

        private void ShowGameOver()
        {
            gameOverScreen.SetActive(true);
        }

        public void UpdateScore(int score)
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {score}";
            }
        }
        
        public void SetBluePlayerName(string playerName)
        {
            if (bluePlayerNameText != null)
            {
                bluePlayerNameText.text = $"Player Blue: {playerName}";
            }
        }
        
        public void SetRedPlayerName(string playerName)
        {
            if (redPlayerNameText != null)
            {
                redPlayerNameText.text = $"Player Red: {playerName}";
            }
        }
        
        public void UpdateBlueNextTetrominoPreview(Vector3[] shape)
        {
            UpdateTetrominoPreview(blueNextTetrominoCells, shape, blueTetrominoColor);
        }
        
        public void UpdateRedNextTetrominoPreview(Vector3[] shape)
        {
            UpdateTetrominoPreview(redNextTetrominoCells, shape, redTetrominoColor);
        }

        /// <summary>
        /// Updates a 3x3 grid preview based on the provided tetromino shape and color.
        /// The method calculates the bounding box of the shape and centers it within the grid.
        /// </summary>
        /// <param name="cells">Array of 9 image cells (ordered row-major: indices 0–2 first row, 3–5 second, 6–8 third).</param>
        /// <param name="shape">Tetromino shape relative positions.</param>
        /// <param name="color">Color to apply for active cells.</param>
        private void UpdateTetrominoPreview(Image[] cells, Vector3[] shape, Color color)
        {
            foreach (var cell in cells)
            {
                cell.color = emptyCellColor;
            }
            
            float minX = shape[0].x, maxX = shape[0].x;
            float minY = shape[0].y, maxY = shape[0].y;
            foreach (var pos in shape)
            {
                if (pos.x < minX) minX = pos.x;
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
                if (pos.y > maxY) maxY = pos.y;
            }

            int shapeWidth = Mathf.RoundToInt(maxX - minX + 1);
            int shapeHeight = Mathf.RoundToInt(maxY - minY + 1);
            
            int gridSize = 3;
            int offsetX = (gridSize - shapeWidth) / 2 - Mathf.RoundToInt(minX);
            int offsetY = (gridSize - shapeHeight) / 2 - Mathf.RoundToInt(minY);
            
            foreach (var pos in shape)
            {
                int cellX = Mathf.RoundToInt(pos.x) + offsetX;
                int cellY = Mathf.RoundToInt(pos.y) + offsetY;
                if (cellX >= 0 && cellX < gridSize && cellY >= 0 && cellY < gridSize)
                {
                    int index = cellY * gridSize + cellX;
                    if (index >= 0 && index < cells.Length)
                    {
                        cells[index].color = color;
                    }
                }
            }
        }
    }
}
