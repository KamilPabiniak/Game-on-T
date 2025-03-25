using Common.Scripts;
using UnityEngine;

namespace TetrisGame.GameLogic
{
    /// <summary>
    /// Sends block position and color data via events.
    /// Attach this component to each game block.
    /// </summary>
    public class BlockDataSender : MonoBehaviour
    {
        private int _blockId;
        private Color _blockColor;

        private void Start()
        {
            _blockId = gameObject.GetInstanceID();
            
            Renderer component = GetComponent<Renderer>();
            _blockColor = component != null ? component.material.color : Color.white;
            
            VisualEvetns.RaiseBlockCreated(_blockId, transform.position, _blockColor);
        }

        private void Update()
        {
            VisualEvetns.RaiseBlockPositionUpdated(_blockId, transform.position, _blockColor);
        }
        
        public void UpdateBlockColor(Color newColor)
        {
            _blockColor = newColor;
        }
    }
}