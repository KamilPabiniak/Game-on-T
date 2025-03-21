using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public struct ServerMessageRpcCommand : IRpcCommand
{
    public FixedString64Bytes message;
}

public struct InitializedClient : IComponentData
{
    
}

public struct PlayerScore : IComponentData
{
    public int Value;
}

public struct GhostGroupElement : IBufferElementData
{
    public Entity Value;
}

[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class ServerSystem : SystemBase
{
    private ComponentLookup<NetworkId> _clients;
    protected override void OnCreate()
    {
        _clients = GetComponentLookup<NetworkId>(true);
    }
    protected override void OnUpdate()
    {
        _clients.Update(this);
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
    
        
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<InitializedClient>().WithEntityAccess())
        {
           commandBuffer.AddComponent<InitializedClient>(entity);
           SendMessageRpc("Client connected with id = " + id.ValueRO.Value, ConnectionManager.ServerWorld);
        }
        commandBuffer.Playback(EntityManager);
        commandBuffer.Dispose();
    }

    public void SendMessageRpc(string text, World world, Entity target = default)
    {
        if (world == null || world.IsCreated == false) return;
        var entity = world.EntityManager.CreateEntity(typeof(SendRpcCommandRequest), typeof(ServerMessageRpcCommand));
        world.EntityManager.SetComponentData(entity, new SendRpcCommandRequest()
        {
            TargetConnection = target
        });
        world.EntityManager.SetComponentData(entity, new ServerMessageRpcCommand
        {
            message = text
        });
    }
}
