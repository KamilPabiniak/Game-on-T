using TetrisGame.Scripts.UI;
using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
{
    [RequireComponent(typeof(Collider))]
    public class BlockCollisionHandler : MonoBehaviour
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
            BlockCollisionHandler otherHandler = collision.gameObject.GetComponent<BlockCollisionHandler>();
            if (otherHandler != null)
            {
                TetrominoController otherController = otherHandler._parentController;
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
            if (otherHandler != null)
            {
                TetrominoController otherController = otherHandler._parentController;
                if (otherController != null && !otherController.isPlaced && otherController.owner != _parentController.owner)
                {
                    _parentController.SwapControlWith(otherController);
                    _hasSwapped = true;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            BlockCollisionHandler otherHandler = collision.gameObject.GetComponent<BlockCollisionHandler>();
            if (otherHandler != null)
            {
                _hasSwapped = false;
            }
        }
    }
}
