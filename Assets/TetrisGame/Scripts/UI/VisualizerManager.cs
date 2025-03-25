using System.Collections.Generic;
using Common.Scripts;
using UnityEngine;

namespace TetrisGame.Scripts.UI
{
    /// <summary>
    /// Manages block visuals in the UI scene.
    /// Listens for block events and creates/updates visual block objects.
    /// </summary>
    public class VisualizerManager : MonoBehaviour
    {
        public GameObject blockVisualPrefab;
        public Transform uiContainer;
        private readonly Dictionary<int, GameObject> _blockVisuals = new();
        
        private Vector3 ConvertWorldPositionToUI(Vector3 worldPosition)
        {
            return worldPosition;
        }

        private void OnEnable()
        {
            VisualEvetns.OnBlockCreated += OnBlockCreated;
            VisualEvetns.OnBlockPositionUpdated += OnBlockPositionUpdated;
            VisualEvetns.OnBlockRemoved += OnBlockRemoved;
        }

        private void OnDisable()
        {
            VisualEvetns.OnBlockCreated -= OnBlockCreated;
            VisualEvetns.OnBlockPositionUpdated -= OnBlockPositionUpdated;
            VisualEvetns.OnBlockRemoved -= OnBlockRemoved;
        }
        
        private void OnBlockCreated(object sender, VisualEvetns.BlockInfoEventArgs e)
        {
            GameObject visual = Instantiate(blockVisualPrefab, uiContainer);
            _blockVisuals[e.BlockId] = visual;
            UpdateVisualPosition(e.BlockId, e.WorldPosition);
            Renderer component = visual.GetComponent<Renderer>();
            if (component != null)
            {
                component.material.color = e.BlockColor;
            }
        }
        
        private void OnBlockPositionUpdated(object sender, VisualEvetns.BlockInfoEventArgs e)
        {
            UpdateVisualPosition(e.BlockId, e.WorldPosition);
            if (_blockVisuals.TryGetValue(e.BlockId, out var visual))
            {
                Renderer component = visual.GetComponent<Renderer>();
                if (component != null)
                {
                    component.material.color = e.BlockColor;
                }
            }
        }
        
        private void OnBlockRemoved(object sender, VisualEvetns.BlockInfoEventArgs e)
        {
            if (_blockVisuals.ContainsKey(e.BlockId))
            {
                Destroy(_blockVisuals[e.BlockId]);
                _blockVisuals.Remove(e.BlockId);
            }
        }
        
        private void UpdateVisualPosition(int blockId, Vector3 worldPosition)
        {
            if (!_blockVisuals.ContainsKey(blockId))
                return;

            Vector3 uiPosition = ConvertWorldPositionToUI(worldPosition);
            _blockVisuals[blockId].transform.position = uiPosition;
        }
    }
}
