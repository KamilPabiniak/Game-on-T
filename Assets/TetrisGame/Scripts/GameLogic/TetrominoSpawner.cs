using System.Collections.Generic;
using UnityEngine;

namespace TetrisGame.Scripts.GameLogic
{
    public class TetrominoSpawner : MonoBehaviour
    {
        //List of shapes here!↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓↓
        private readonly List<Vector3[]> _tetrominoShapes = new()
        {
            // I
            new Vector3[]
            {
                new (-1, 0, 0),
                new (0, 0, 0),
                new (1, 0, 0),
                new (2, 0, 0)
            },
            // O
            new Vector3[]
            {
                new (0, 0, 0),
                new (1, 0, 0),
                new (0, 1, 0),
                new (1, 1, 0)
            },
            // T
            new Vector3[]
            {
                new (0, 0, 0),
                new (-1, 0, 0),
                new (1, 0, 0),
                new (0, 1, 0)
            },
            // L
            new Vector3[]
            {
                new (0, 0, 0),
                new (0, 1, 0),
                new (0, 2, 0),
                new (1, 2, 0)
            },
            // Z
            new Vector3[]
            {
                new(0, 0, 0),
                new(1, 0, 0),
                new(0, 1, 0),
                new(-1, 1, 0)
            },
            // ?
            new Vector3[]
            {
                new(0, 0, 0),
                new(-1, 0, 0),
                new(0, 1, 0),
                new(1, 1, 0)
            }
        };

        [Tooltip("Prefab for the tetromino container (must have TetrominoContainer component. REMEMBER THAT)")]
        public GameObject tetrominoContainerPrefab;
        
        public Vector3[] GetRandomTetrominoShape()
        {
            int shapeIndex = Random.Range(0, _tetrominoShapes.Count);
            return _tetrominoShapes[shapeIndex];
        }
    }
}
