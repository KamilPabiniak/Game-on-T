using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace GameUI
{
    /// <summary>
    /// Manages the Tetris UI: displays score, next tetromino, and player name.
    /// </summary>
    public class ClientUIManager : MonoBehaviour
    {
        [Tooltip("UI Text for displaying the current score.")]
        public TextMeshProUGUI ScoreText;

        [Tooltip("UI Text for displaying the next tetromino.")]
        public TextMeshProUGUI NextTetrominoText;

        [Tooltip("UI Text for displaying the player's name.")]
        public TextMeshProUGUI PlayerNameText;

        private int _score;
        private string _nextTetromino;
        private string _playerName;

        private void Start()
        {
            _score = 0;
            _nextTetromino = "N/A";
            _playerName = "Player";
            UpdateUI();
        }
        
        public void UpdateScore(int newScore)
        {
            _score = newScore;
            UpdateUI();
        }

        public void SetNextTetromino(string tetrominoName)
        {
            _nextTetromino = tetrominoName;
            UpdateUI();
        }


        public void SetPlayerName(string playerName)
        {
            _playerName = playerName;
            UpdateUI();
        }

        private void UpdateUI()
        {
            if (ScoreText != null)
                ScoreText.text = "Score: " + _score;
            if (NextTetrominoText != null)
                NextTetrominoText.text = "Next: " + _nextTetromino;
            if (PlayerNameText != null)
                PlayerNameText.text = "Player: " + _playerName;
        }
    }
}