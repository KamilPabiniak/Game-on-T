using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
{
    public class GridSnap : MonoBehaviour
    {
        private void Update()
        {
            var position = transform.position;
            Vector3 snappedPos = new Vector3(
                Mathf.Round(position.x),
                Mathf.Round(position.y),
                0
            );
            position = snappedPos;
            transform.position = position;
        }
    }
}