using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;

public struct GoInGameCommand : IRpcCommand
{
    
}

[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
partial struct GoInGameClientSystem : ISystem
{
    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAny<NetworkId>();
        builder.WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (id, entity) in SystemAPI.Query<RefRO<NetworkId>>().WithNone<NetworkStreamInGame>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(entity);
            var request = commandBuffer.CreateEntity();
            commandBuffer.AddComponent<GoInGameCommand>(request);
            commandBuffer.AddComponent<SendRpcCommandRequest>(request);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
    
}
