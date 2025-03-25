using TetrisGame.Scripts.UI;
using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance;
        public TetrominoSpawner tetrominoSpawner;
        public Transform[] spawnPointsBlue;
        public Transform[] spawnPointsRed;
        
        private Vector3[] _nextTetrominoShapeBlue;
        private Vector3[] _nextTetrominoShapeRed;
        
        private bool _bluePlaced;
        private bool _redPlaced;
        
        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            _nextTetrominoShapeBlue = tetrominoSpawner.GetRandomTetrominoShape();
            _nextTetrominoShapeRed = tetrominoSpawner.GetRandomTetrominoShape();

            TetrisUIManager.Instance.UpdateBlueNextTetrominoPreview(_nextTetrominoShapeBlue);
            TetrisUIManager.Instance.UpdateRedNextTetrominoPreview(_nextTetrominoShapeRed);

            TetrisUIManager.Instance.SetBluePlayerName("BluePlayer");
            TetrisUIManager.Instance.SetRedPlayerName("RedPlayer");

            SpawnTetrominos();
        }
        
        private void SpawnTetrominos()
        {
            int indexBlue = Random.Range(0, spawnPointsBlue.Length);
            SpawnTetrominoForPlayer(spawnPointsBlue[indexBlue].position, PlayerColor.Blue, _nextTetrominoShapeBlue);
            _nextTetrominoShapeBlue = tetrominoSpawner.GetRandomTetrominoShape();
            TetrisUIManager.Instance.UpdateBlueNextTetrominoPreview(_nextTetrominoShapeBlue);
            
            int indexRed = Random.Range(0, spawnPointsRed.Length);
            SpawnTetrominoForPlayer(spawnPointsRed[indexRed].position, PlayerColor.Red, _nextTetrominoShapeRed);
            _nextTetrominoShapeRed = tetrominoSpawner.GetRandomTetrominoShape();
            TetrisUIManager.Instance.UpdateRedNextTetrominoPreview(_nextTetrominoShapeRed);
            
            _bluePlaced = false;
            _redPlaced = false;
        }
        
        private void SpawnTetrominoForPlayer(Vector3 spawnPosition, PlayerColor owner, Vector3[] shape)
        {
            GameObject container = Instantiate(tetrominoSpawner.tetrominoContainerPrefab);
            container.name = "TetrominoContainer";
            container.transform.position = spawnPosition;

            TetrominoContainer tetrominoContainer = container.GetComponent<TetrominoContainer>();
            if (tetrominoContainer != null)
            {
                tetrominoContainer.Initialize(shape);
            }
            else
            {
                Debug.LogError("TetrominoContainer prefab missing TetrominoContainer component!");
            }

            TetrominoController controller = container.GetComponent<TetrominoController>();
            if (controller == null)
            {
                controller = container.AddComponent<TetrominoController>();
            }
            controller.owner = owner;
            controller.Initialize();
        }
        
        public void OnTetrominoPlaced(PlayerColor owner)
        {
            if (owner == PlayerColor.Blue)
                _bluePlaced = true;
            else if (owner == PlayerColor.Red)
                _redPlaced = true;
            
            if (_bluePlaced && _redPlaced)
            {
                SpawnTetrominos();
            }
        }
    }
}
