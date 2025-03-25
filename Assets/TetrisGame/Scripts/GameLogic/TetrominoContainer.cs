using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
{
    public class TetrominoContainer : MonoBehaviour
    {
        [Tooltip("Prefab for an individual block (should have a Collider and use physics)")]
        public GameObject blockPrefab;
        
        public void Initialize(Vector3[] shapeDefinition)
        {
            ClearChildren();
            
            foreach (Vector3 offset in shapeDefinition)
            {
                CreateBlock(offset);
            }
        }
        
        private void CreateBlock(Vector3 localPosition)
        {
            GameObject block = Instantiate(blockPrefab, transform);
            block.transform.localPosition = localPosition;
            block.transform.rotation = Quaternion.identity;
            
            if (block.GetComponent<Collider>() == null)
            {
                BoxCollider collider = block.AddComponent<BoxCollider>();
                collider.size = Vector3.one * 0.95f;
            }
            
        }
        
        private void ClearChildren()
        {
            while (transform.childCount > 0)
            {
                DestroyImmediate(transform.GetChild(0).gameObject);
            }
        }
    }
}
