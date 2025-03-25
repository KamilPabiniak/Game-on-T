using System.Collections.Generic;
using TetrisGame.Scripts.GameLogic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TetrisGame.Scripts.UI
{
    [RequireComponent(typeof(Rigidbody))]
    public class TetrominoController : MonoBehaviour
    {
        private static readonly HashSet<(TetrominoController, TetrominoController)> SwappedPairs = new();

        [Header("Movement Settings")]
        public float horizontalSpeed = 5f;
        public float fallSpeed = 3f;
        public bool isPlaced;

        // Input keys for movement
        public Key blueMoveLeftKey = Key.LeftArrow;
        public Key blueMoveRightKey = Key.RightArrow;
        public Key blueFastFallKey = Key.DownArrow;

        public Key redMoveLeftKey = Key.A;
        public Key redMoveRightKey = Key.D;
        public Key redFastFallKey = Key.S;

        [Header("Rotation Keys (Opposing Player)")]
        public Key blueRotateKey = Key.W;
        public Key redRotateKey = Key.UpArrow;

        public PlayerColor owner = PlayerColor.Blue;

        private Rigidbody _rigidbody;
        private Vector3 _movement;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.isKinematic = false;
            _rigidbody.useGravity = false;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX |
                                     RigidbodyConstraints.FreezeRotationY |
                                     RigidbodyConstraints.FreezeRotationZ;
        }

        /// <summary>
        ///  Set tetromino color there
        /// </summary>
        public void Initialize()
        {
            SetTetrominoColor();
        }

        private void Update()
        {
            HandleInput();
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

            // Check if any block has reached the bottom of the board
            foreach (Transform block in transform)
            {
                if (Mathf.RoundToInt(block.position.y) <= 0)
                {
                    SnapBlocksToGrid();
                    PlaceTetromino();
                    return;
                }
            }

            // If moving down is not possible, move tetromino downward step-by-step
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
        
        /// <summary>
        /// Snaps (attaches) all tetromino blocks to the nearest grid cell centers
        /// </summary>
        public void SnapBlocksToGrid()
        {
            foreach (Transform block in transform)
            {
                var position = block.position;
                int gridX = Mathf.RoundToInt(position.x);
                int gridY = Mathf.RoundToInt(position.y);
                position = new Vector3(gridX, gridY , position.z);
                block.position = position;
            }
        }
        
        private void HandleInput()
        {
            Vector3 movement = Vector3.zero;

            if (owner == PlayerColor.Blue)
            {
                if (Keyboard.current[blueMoveLeftKey].isPressed)
                    movement.x = -horizontalSpeed;
                if (Keyboard.current[blueMoveRightKey].isPressed)
                    movement.x = horizontalSpeed;
                movement.y = Keyboard.current[blueFastFallKey].isPressed ? -fallSpeed * 2.5f : -fallSpeed;
            }
            else // Owner Red
            {
                if (Keyboard.current[redMoveLeftKey].isPressed)
                    movement.x = -horizontalSpeed;
                if (Keyboard.current[redMoveRightKey].isPressed)
                    movement.x = horizontalSpeed;
                movement.y = Keyboard.current[redFastFallKey].isPressed ? -fallSpeed * 2.5f : -fallSpeed;
            }

            _movement = movement;
        }

 
        private void HandleRotation()
        {
            bool rotatePressed = false;
            if (owner == PlayerColor.Blue && Keyboard.current[blueRotateKey].wasPressedThisFrame)
            {
                rotatePressed = true;
            }
            else if (owner == PlayerColor.Red && Keyboard.current[redRotateKey].wasPressedThisFrame)
            {
                rotatePressed = true;
            }

            if (!rotatePressed)
                return;

            // Calculate new local positions after 90° rotation about Z-axis: newLocal = (y, -x, z)
            List<Vector3> newLocalPositions = new List<Vector3>();
            foreach (Transform block in transform)
            {
                Vector3 currentLocal = block.localPosition;
                Vector3 newLocal = new Vector3(currentLocal.y, -currentLocal.x, currentLocal.z);
                newLocalPositions.Add(newLocal);
            }

            // Validate that all new positions are within board boundaries and not occupied
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

        /// <summary>
        /// Checks if the tetromino can move in the given direction
        /// </summary>
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

        /// <summary>
        /// Finalizes the tetromino placement: snaps blocks to the grid, detaches them from the container,
        /// sets their layer to "Snaped", informs the GameManager and destroys the container. Wow
        /// </summary>
        public void PlaceTetromino()
        {
            isPlaced = true;
            BoardManager.Instance.PlaceTetromino(gameObject);
            GameManager.Instance.OnTetrominoPlaced(owner);
        }

        /// <summary>
        /// Sets color lol
        /// </summary>
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
