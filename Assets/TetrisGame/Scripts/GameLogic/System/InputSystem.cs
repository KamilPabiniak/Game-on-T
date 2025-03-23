using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GamePresentation
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class InputSystem : SystemBase
    {
        private Controls _controls;

        protected override void OnCreate()
        {
            _controls = new Controls();
            _controls.Enable();
        }

        protected override void OnUpdate()
        {
            Vector2 moveInput = _controls.PlayerInput.Movement.ReadValue<Vector2>();
            bool rotateInput = _controls.PlayerInput.Rotate.triggered;

            Entities
                .WithAll<GhostOwnerIsLocal>()
                .ForEach((ref PlayerInputData input) =>
                {
                    input.Move = moveInput;
                    input.Rotate = rotateInput;
                }).ScheduleParallel();
        }

        protected override void OnDestroy()
        {
            _controls.Disable();
        }
    }
}