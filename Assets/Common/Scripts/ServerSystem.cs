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
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<ClientMessageRpcCommand>>().WithEntityAccess())
        {
            Debug.Log(command.ValueRO.message + " from client index " + request.ValueRO.SourceConnection.Index + " version " + request.ValueRO.SourceConnection.Version);
            commandBuffer.DestroyEntity(entity);
        }

        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<SpawnBlockRpcCommand>>().WithEntityAccess())
        {
            if (SystemAPI.TryGetSingletonBuffer<PrefabElement>(out var prefabBuffer) && prefabBuffer.Length > 0)
            {
                var prefabEntity = prefabBuffer[Random.Range(0, prefabBuffer.Length)].Value;
                Entity unit = commandBuffer.Instantiate(prefabEntity);
                commandBuffer.SetComponent(unit, new LocalTransform()
                {
                    Position = new float3(Random.Range(-10f, 10f), 0, 0),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                var networkId = _clients[request.ValueRO.SourceConnection]; 
                commandBuffer.SetComponent(unit, new GhostOwner
                {
                    NetworkId = networkId.Value
                });
                
                commandBuffer.AppendToBuffer(request.ValueRO.SourceConnection, new LinkedEntityGroup()
                {
                    Value = unit
                });
                commandBuffer.DestroyEntity(entity);
            }
        }
        
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
