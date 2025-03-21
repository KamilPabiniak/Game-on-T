using Unity.Entities;
using Unity.Transforms;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameLogic
{
    public partial class ClientInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float moveAmount = 0f;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                if (keyboard.leftArrowKey.wasPressedThisFrame)
                    moveAmount = -1f;
                else if (keyboard.rightArrowKey.wasPressedThisFrame)
                    moveAmount = 1f;
            }

            if (moveAmount == 0f)
                return;
            
            Entities.WithAll<FallingTetromino>().WithoutBurst().ForEach((ref LocalTransform transform) =>
            {
                float newX = transform.Position.x + moveAmount;
                if (newX >= 0 && newX <= TetrisGrid.Width - 1)
                {
                    transform.Position.x = newX;
                }
            }).Run();
        }
    }
}