using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;


[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
partial struct GoInGameServerSystem : ISystem
{
    //[BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        var builder = new EntityQueryBuilder(Allocator.Temp);
        builder.WithAll<ReceiveRpcCommandRequest, GoInGameCommand>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    //[BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
        foreach (var (request, command, entity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>, RefRO<GoInGameCommand>>().WithEntityAccess())
        {
            commandBuffer.AddComponent<NetworkStreamInGame>(request.ValueRO.SourceConnection);
            commandBuffer.DestroyEntity(entity);
        }
        commandBuffer.Playback(state.EntityManager);
        commandBuffer.Dispose();
    }
}
