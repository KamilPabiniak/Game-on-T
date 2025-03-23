using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using UnityEngine.InputSystem;

[UpdateInGroup(typeof(GhostInputSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
public partial class InputSystem : SystemBase
{
    private Controls _controls;

    protected override void OnCreate()
    {
        Debug.Log("InputSystem created");
        _controls = new Controls();
        _controls.Enable(); 
        RequireForUpdate(GetEntityQuery(ComponentType.ReadOnly<PlayerInputData>(), ComponentType.ReadOnly<GhostOwnerIsLocal>()));
    }

    protected override void OnUpdate()
    {
        Debug.Log("InputSystem updating");
        Vector2 playerMove = _controls.PlayerInput.Movement.ReadValue<Vector2>();
        Debug.Log($"Raw input received: {playerMove}");

        var query = GetEntityQuery(ComponentType.ReadWrite<PlayerInputData>(), ComponentType.ReadOnly<GhostOwnerIsLocal>());
        Debug.Log($"Entities with PlayerInputData and GhostOwnerIsLocal: {query.CalculateEntityCount()}");

        foreach (var input in SystemAPI.Query<RefRW<PlayerInputData>>().WithAll<GhostOwnerIsLocal>())
        {
            input.ValueRW.move = playerMove;
            Debug.Log($"Input assigned to entity: {playerMove}");
        }
    }

    protected override void OnDestroy()
    {
        _controls.Disable();
    }
}