using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Unity.NetCode;

namespace GameLogic
{
    public struct InitializedClient : IComponentData
    {
    }

    public struct TetrominoContainer : IComponentData
    {
    }

    public struct TetrominoBlock : IBufferElementData
    {
        public int2 Offset;
    }

    public struct TetrominoOwner : IComponentData
    {
        public int NetworkId;
    }

    public struct TetrominoBlockLink : IComponentData
    {
        public Entity Container;
        public int BlockIndex;
    }

    public struct TetrominoPlaced : IComponentData
    {
    }

    public struct VisualSpawnedTag : IComponentData
    {
    }

    [BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
public partial class TetrominoSpawnAndFollowSystem : SystemBase
{
    public float SpawnInterval = 3f;
    private float _timeSinceLastSpawn;

    protected override void OnUpdate()
    {
        float deltaTime = SystemAPI.Time.DeltaTime;
        _timeSinceLastSpawn += deltaTime;

        EntityQuery spawnPointQuery = GetEntityQuery(ComponentType.ReadOnly<TetrominoSpawnPoint>());
        NativeArray<TetrominoSpawnPoint> spawnPoints = spawnPointQuery.ToComponentDataArray<TetrominoSpawnPoint>(Allocator.TempJob);
        if (spawnPoints.Length == 0)
        {
            spawnPoints.Dispose();
            return;
        }

        EntityQuery fallingQuery = GetEntityQuery(ComponentType.ReadOnly<FallingTetromino>());
        if (_timeSinceLastSpawn >= SpawnInterval && fallingQuery.CalculateEntityCount() == 0)
        {
            _timeSinceLastSpawn = 0f;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

            if (SystemAPI.TryGetSingleton<PrefabElement>(out var prefabElement))
            {
                EntityQuery networkIdQuery = GetEntityQuery(ComponentType.ReadOnly<NetworkId>());
                NativeArray<NetworkId> connectionIds = networkIdQuery.ToComponentDataArray<NetworkId>(Allocator.TempJob);
                if (connectionIds.Length == 0)
                {
                    connectionIds.Dispose();
                    spawnPoints.Dispose();
                    return;
                }

                using (var ecb = new EntityCommandBuffer(Allocator.Temp))
                {
                    for (int i = 0; i < connectionIds.Length; i++)
                    {
                        int netId = connectionIds[i].Value;
                        Debug.Log($"netId: {netId}, connectionIds[i].Value: {connectionIds[i].Value}");

                        TetrominoSpawnPoint chosenSpawn = spawnPoints[0];
                        for (int j = 0; j < spawnPoints.Length; j++)
                        {
                            if (spawnPoints[j].PlayerId == netId)
                            {
                                chosenSpawn = spawnPoints[j];
                                break;
                            }
                        }

                        float3 spawnPosition = chosenSpawn.Position;
                        Entity container = ecb.CreateEntity();
                        ecb.AddComponent(container, new TetrominoContainer());
                        ecb.AddComponent(container, new FallingTetromino());
                        Debug.Log($"FallingTetromino added to Tetromino for NetworkId: {netId}");
                        ecb.AddComponent(container, new FallSpeed { Value = 2f });
                        ecb.AddComponent(container, new LocalTransform
                        {
                            Position = spawnPosition,
                            Rotation = quaternion.identity,
                            Scale = 1f
                        });
                        ecb.AddComponent(container, new LocalToWorld());
                        ecb.AddComponent<GhostOwner>(container); // Dodaj GhostOwner

                        if (netId == connectionIds[i].Value)
                        {
                            ecb.AddComponent<GhostOwnerIsLocal>(container);
                            Debug.Log($"GhostOwnerIsLocal assigned to Tetromino for NetworkId: {netId}");
                        }

                        ecb.AddComponent<PlayerInputData>(container);
                        Debug.Log($"PlayerInputData added to Tetromino for NetworkId: {netId}");

                        DynamicBuffer<TetrominoBlock> blockBuffer = ecb.AddBuffer<TetrominoBlock>(container);
                        NativeList<int2> offsets = new NativeList<int2>(Allocator.Temp);
                        int shapeIndex = UnityEngine.Random.Range(0, 2);
                        if (shapeIndex == 0)
                        {
                            offsets.Add(new int2(0, 0));
                            offsets.Add(new int2(1, 0));
                            offsets.Add(new int2(0, 1));
                            offsets.Add(new int2(1, 1));
                        }
                        else
                        {
                            offsets.Add(new int2(0, 0));
                            offsets.Add(new int2(1, 0));
                            offsets.Add(new int2(2, 0));
                            offsets.Add(new int2(3, 0));
                        }

                        for (int j = 0; j < offsets.Length; j++)
                        {
                            int2 offset = offsets[j];
                            blockBuffer.Add(new TetrominoBlock { Offset = offset });
                            Entity blockEntity = ecb.Instantiate(prefabElement.Value);
                            ecb.AddComponent(blockEntity, new TetrominoBlockLink { Container = container, BlockIndex = j });
                            ecb.SetComponent(blockEntity, new LocalTransform
                            {
                                Position = spawnPosition + new float3(offset.x, offset.y, 0f),
                                Rotation = quaternion.identity,
                                Scale = 1f
                            });
                            ecb.AddComponent<LocalToWorld>(blockEntity); // Dodaj LocalToWorld
                        }
                        offsets.Dispose();

                        Debug.Log($"Tetromino spawned for NetworkId {netId} at {spawnPosition}");
                    }
                    ecb.Playback(entityManager);
                }
                connectionIds.Dispose();
            }
        }
        spawnPoints.Dispose();
    }
}
}


