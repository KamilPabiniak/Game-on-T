using System.Collections.Generic;
using TetrisGame.Scripts.UI;
using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
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
        private Vector3 _movement;
        private TetrominoInputHandler _inputHandler;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
                                     RigidbodyConstraints.FreezeRotationY |
                                     RigidbodyConstraints.FreezeRotationZ;
            
            _inputHandler = GetComponent<TetrominoInputHandler>();
        }
        
        public void Initialize()
        {
            SetTetrominoColor();
        }

        private void Update()
        {
            _movement = _inputHandler != null 
                ? _inputHandler.GetMovement(owner, horizontalSpeed, fallSpeed) 
                : Vector3.zero;
            
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
                if (Mathf.RoundToInt(block.position.y) <= 0)
                {
                    SnapBlocksToGrid();
                    PlaceTetromino();
                    return;
                }
            }
            
            if (!CanMove(Vector3.down * (fallSpeed * Time.fixedDeltaTime)))
            {
                while (CanMove(Vector3.down))
                {
                    _rigidbody.MovePosition(_rigidbody.position + Vector3.down);
                }
                SnapBlocksToGrid();
                PlaceTetromino();
                return;
            }

            _rigidbody.MovePosition(_rigidbody.position + _movement * Time.fixedDeltaTime);
        }
        
        public void SnapBlocksToGrid()
        {
            foreach (Transform block in transform)
            {
                Vector3 position = block.position;
                int gridX = Mathf.RoundToInt(position.x);
                int gridY = Mathf.RoundToInt(position.y);
                block.position = new Vector3(gridX, gridY, position.z);
            }
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
                int gridY = Mathf.RoundToInt(newWorldPos.y);

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
                    component.material.color = color;
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
