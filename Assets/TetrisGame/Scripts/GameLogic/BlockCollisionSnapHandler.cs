using UnityEngine;

namespace TetrisGame.GameLogic
{
    [RequireComponent(typeof(Collider))]
    public class BlockCollisionSnapHandler : MonoBehaviour
    {
        private TetrominoController _parentController;
        private bool _hasSwapped;

        private void Awake()
        {
            _parentController = GetComponentInParent<TetrominoController>();
            GetComponent<Collider>().isTrigger = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            BlockCollisionSnapHandler otherSnapHandler = collision.gameObject.GetComponent<BlockCollisionSnapHandler>();
            if (otherSnapHandler != null)
            {
                TetrominoController otherController = otherSnapHandler._parentController;
                if (otherController != null && otherController.isPlaced)
                {
                    if (!_parentController.isPlaced)
                    {
                        _parentController.SnapBlocksToGrid();
                        _parentController.PlaceTetromino();
                    }
                    return;
                }
            }
            
            if (_hasSwapped) return;
            if (otherSnapHandler != null)
            {
                TetrominoController otherController = otherSnapHandler._parentController;
                if (otherController != null && !otherController.isPlaced && otherController.owner != _parentController.owner)
                {
                    _parentController.SwapControlWith(otherController);
                    _hasSwapped = true;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            BlockCollisionSnapHandler otherSnapHandler = collision.gameObject.GetComponent<BlockCollisionSnapHandler>();
            if (otherSnapHandler != null)
            {
                _hasSwapped = false;
            }
        }
    }
}
