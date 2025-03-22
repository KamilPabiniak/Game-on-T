using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GameLogic
{
    public partial class ClientInputSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            float hostMove = 0f;
            float clientMove = 0f;
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                // Sterowanie hosta – strzałki
                if (keyboard.leftArrowKey.wasPressedThisFrame)
                    hostMove = -1f;
                else if (keyboard.rightArrowKey.wasPressedThisFrame)
                    hostMove = 1f;
                
                // Sterowanie klienta – A/D
                if (keyboard.aKey.wasPressedThisFrame)
                    clientMove = -1f;
                else if (keyboard.dKey.wasPressedThisFrame)
                    clientMove = 1f;
            }
            
            if (hostMove != 0f)
            {
                // Aktualizujemy tylko obiekty dla hosta (PlayerId == 0)
                Entities
                    .WithAll<FallingTetromino>()
                    .WithoutBurst()
                    .ForEach((ref LocalTransform transform, in PlayerControl control) =>
                    {
                        if (control.PlayerId == 0)
                        {
                            float newX = transform.Position.x + hostMove;
                            if (newX >= 0 && newX <= TetrisGrid.Width - 1)
                                transform.Position.x = newX;
                        }
                    }).Run();
            }
            
            if (clientMove != 0f)
            {
                // Aktualizujemy tylko obiekty dla klienta (PlayerId == 1)
                Entities
                    .WithAll<FallingTetromino>()
                    .WithoutBurst()
                    .ForEach((ref LocalTransform transform, in PlayerControl control) =>
                    {
                        if (control.PlayerId == 1)
                        {
                            float newX = transform.Position.x + clientMove;
                            if (newX >= 0 && newX <= TetrisGrid.Width - 1)
                                transform.Position.x = newX;
                        }
                    }).Run();
            }
        }
    }
}
