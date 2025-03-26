using System.Collections.Generic;
using TetrisGame.Scripts.UI;
using UnityEngine;

namespace TetrisGame.GameLogic
{
    [RequireComponent(typeof(Rigidbody))]
    public class TetrominoController : MonoBehaviour
    {
        private static readonly HashSet<(TetrominoController, TetrominoController)> SwappedPairs = new();

        [Header("Movement Settings")]
        public float horizontalSpeed = 5f;
        public float fallSpeed = 3f;
        public bool isPlaced;
        public PlayerColor owner = PlayerColor.Blue;

        private Rigidbody _rigidbody;
        private TetrominoInputHandler _inputHandler;
        private Vector3 _movement;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _inputHandler = GetComponent<TetrominoInputHandler>();
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        }
        
        public void Initialize()
        {
            SetTetrominoColor();
        }

        private void Update()
        {
            if (isPlaced) return;
            
            HandleMovement();
            HandleRotation();
        }
        
        private void LateUpdate()
        {
            SwappedPairs.Clear();
        }

        private void FixedUpdate()
        {
            if (isPlaced)
                return;
    
            foreach (Transform block in transform)
            {
                if (Mathf.FloorToInt(block.position.y) <= 0)
                {
                    SnapBlocksToGrid();
                    PlaceTetromino();
                    return;
                }
            }
            
            Vector3 fallStep = Vector3.down * (fallSpeed * GameManager.Instance.GlobalFallSpeed * Time.fixedDeltaTime);
    
            if (!CanMove(fallStep))
            {
                const float smoothStep = 0.05f;
                Vector3 newPos = _rigidbody.position;
                int iteration = 0;
                int maxIterations = 10;
                while (CanMove(Vector3.down * smoothStep) && iteration < maxIterations)
                {
                    newPos += Vector3.down * smoothStep;
                    iteration++;
                }
                _rigidbody.MovePosition(newPos);
                SnapBlocksToGrid();
                PlaceTetromino();
                return;
            }
    
            Vector3 targetPosition = _rigidbody.position + _movement * Time.fixedDeltaTime;
            float blendFactor = 0.9f;
            Vector3 smoothPosition = Vector3.Lerp(_rigidbody.position, targetPosition, blendFactor);
            _rigidbody.MovePosition(smoothPosition);
        }


        
        public void SnapBlocksToGrid()
        {
            Vector3 boardOrigin = BoardManager.Instance.transform.position;
            int boardWidth = BoardManager.Instance.boardWidth;
            int boardHeight = BoardManager.Instance.boardHeight;

            List<Vector3> newPositions = new List<Vector3>();

            foreach (Transform block in transform)
            {
                var position = block.position;
                float relX = position.x - boardOrigin.x;
                float relY = position.y - boardOrigin.y;
                int cellX = Mathf.Clamp(Mathf.RoundToInt(relX), 0, boardWidth - 1);
                int cellY = Mathf.Clamp(Mathf.RoundToInt(relY), 0, boardHeight - 1);
                newPositions.Add(boardOrigin + new Vector3(cellX, cellY, position.z));
            }

            for (int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).position = newPositions[i];
            }
        }

        private void HandleMovement()
        {
            _movement = _inputHandler != null ? _inputHandler.GetMovement(owner, horizontalSpeed, 
                fallSpeed * GameManager.Instance.GlobalFallSpeed) : Vector3.zero;
        }
        
        private void HandleRotation()
        {
            bool rotatePressed = _inputHandler != null && _inputHandler.IsRotatePressed(owner);

            if (!rotatePressed)
                return;

            List<Vector3> newLocalPositions = new List<Vector3>();
            foreach (Transform block in transform)
            {
                Vector3 currentLocal = block.localPosition;
                Vector3 newLocal = new Vector3(currentLocal.y, -currentLocal.x, currentLocal.z);
                newLocalPositions.Add(newLocal);
            }

            bool canRotate = true;
            for (int i = 0; i < transform.childCount; i++)
            {
                Vector3 newWorldPos = transform.position + newLocalPositions[i];
                int gridX = Mathf.RoundToInt(newWorldPos.x);
                int gridY = Mathf.FloorToInt(newWorldPos.y);

                if (gridX < 0 || gridX >= BoardManager.Instance.boardWidth ||
                    gridY < 0 || gridY >= BoardManager.Instance.boardHeight)
                {
                    canRotate = false;
                    break;
                }
                if (BoardManager.Instance.IsPositionOccupied(new Vector2Int(gridX, gridY)))
                {
                    canRotate = false;
                    break;
                }
            }

            if (canRotate)
            {
                int index = 0;
                foreach (Transform block in transform)
                {
                    block.localPosition = newLocalPositions[index];
                    index++;
                }
            }
        }
        
        private bool CanMove(Vector3 direction)
        {
            foreach (Transform block in transform)
            {
                Vector3 newPos = block.position + direction;
                int newX = Mathf.RoundToInt(newPos.x);
                int newY = Mathf.FloorToInt(newPos.y);
                Vector2Int gridPos = new Vector2Int(newX, newY);

                if (gridPos.y < 0 || BoardManager.Instance.IsPositionOccupied(gridPos))
                    return false;
            }
            return true;
        }

        public void PlaceTetromino()
        {
            isPlaced = true;
            BoardManager.Instance.PlaceTetromino(gameObject);
            GameManager.Instance.OnTetrominoPlaced(owner);
        }
        
        private void SetTetrominoColor()
        {
            Color color = owner == PlayerColor.Blue ? Color.blue : Color.red;
            foreach (Transform child in transform)
            {
                Renderer component = child.GetComponent<Renderer>();
                if (component != null)
                {
                    component.material.color = color;
                }
                BlockDataSender sender = child.GetComponent<BlockDataSender>();
                if (sender != null)
                {
                    sender.UpdateBlockColor(color);
                }
            }
        }
        
        public void SwapControlWith(TetrominoController other)
        {
            (owner, other.owner) = (other.owner, owner);
            SetTetrominoColor();
            other.SetTetrominoColor();
        }
        
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.layer == LayerMask.NameToLayer("Snaped"))
            {
                if (!isPlaced)
                {
                    SnapBlocksToGrid();
                    PlaceTetromino();
                }
                return;
            }
            
            TetrominoController otherController = collision.gameObject.GetComponentInParent<TetrominoController>();
            if (otherController != null && otherController != this && otherController.owner != owner)
            {
                _rigidbody.linearVelocity = Vector3.zero;

                var pair = (this, otherController);
                var reversePair = (otherController, this);

                if (!SwappedPairs.Contains(pair) && !SwappedPairs.Contains(reversePair))
                {
                    SwapControlWith(otherController);
                    SetTetrominoColor();
                    SwappedPairs.Add(pair);
                }
            }
        }
    }
}
