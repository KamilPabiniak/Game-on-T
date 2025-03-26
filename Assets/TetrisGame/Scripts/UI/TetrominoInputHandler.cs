using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TetrisGame.Scripts.UI
{
    public enum PlayerColor
    {
        Blue,   // Player 1
        Red     // Player 2
    }
    public class TetrominoInputHandler : MonoBehaviour
    {
        [Header("Blue Player Keys")]
        public Key blueMoveLeftKey = Key.A;
        public Key blueMoveRightKey = Key.D;
        public Key blueFastFallKey = Key.S;
        public Key blueRotateKey = Key.UpArrow;

        [Header("Red Player Keys")]
        public Key redMoveLeftKey = Key.LeftArrow;
        public Key redMoveRightKey = Key.RightArrow;
        public Key redFastFallKey = Key.DownArrow;
        public Key redRotateKey = Key.W;
        
        public Key quitKey = Key.Escape;

        private void Update()
        {
            if (Keyboard.current[quitKey].isPressed)
            {
                Application.Quit();
            }
        }

        public Vector3 GetMovement(PlayerColor owner, float horizontalSpeed, float fallSpeed)
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
            else 
            {
                if (Keyboard.current[redMoveLeftKey].isPressed)
                    movement.x = -horizontalSpeed;
                if (Keyboard.current[redMoveRightKey].isPressed)
                    movement.x = horizontalSpeed;
                movement.y = Keyboard.current[redFastFallKey].isPressed ? -fallSpeed * 2.5f : -fallSpeed;
            }

            return movement;
        }


        public bool IsRotatePressed(PlayerColor owner)
        {
            if (owner == PlayerColor.Blue)
                return Keyboard.current[blueRotateKey].wasPressedThisFrame;
            else
                return Keyboard.current[redRotateKey].wasPressedThisFrame;
        }
    }
}
