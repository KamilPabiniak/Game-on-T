using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    // Komponenty używane przez system
    public struct TetrominoContainer : IComponentData { }
    public struct TetrominoBlock : IBufferElementData { public int2 Offset; }
    public struct TetrominoBlockLink : IComponentData { public Entity Container; public int BlockIndex; }
    public struct PlayerControl : IComponentData { public int PlayerId; }

    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoSpawnAndFollowSystem : SystemBase
    {
        public float SpawnInterval = 3f;
        private float _timeSinceLastSpawn;

        protected override void OnCreate()
        {
            RequireForUpdate<PrefabElement>();
        }

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            _timeSinceLastSpawn += deltaTime;

            // Pobieramy wszystkie spawn pointy z sceny
            EntityQuery spawnPointQuery = GetEntityQuery(ComponentType.ReadOnly<TetrominoSpawnPoint>());
            NativeArray<TetrominoSpawnPoint> spawnPoints = spawnPointQuery.ToComponentDataArray<TetrominoSpawnPoint>(Allocator.TempJob);

            if (spawnPoints.Length == 0)
            {
                Debug.LogWarning("Nie znaleziono spawn pointów w scenie!");
                spawnPoints.Dispose();
                return;
            }

            // Sprawdzamy, czy nie leci już jakaś figura
            EntityQuery fallingQuery = GetEntityQuery(ComponentType.ReadOnly<FallingTetromino>());
            if (_timeSinceLastSpawn >= SpawnInterval && fallingQuery.CalculateEntityCount() == 0)
            {
                _timeSinceLastSpawn = 0f;
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                // Pobieramy prefabrykat z singletonu
                if (SystemAPI.TryGetSingleton<PrefabElement>(out var prefabElement))
                {
                    Debug.Log($"PrefabElement singleton found: {prefabElement.Value}");
                    using (var ecb = new EntityCommandBuffer(Allocator.Temp))
                    {
                        // Zakładamy, że mamy dwóch graczy (możesz to zmodyfikować według potrzeb)
                        for (int playerId = 0; playerId < 2; playerId++)
                        {
                            // Dla testów wybieramy losowo dowolny spawn point
                            int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
                            float3 spawnPosition = spawnPoints[randomIndex].Position;

                            // Tworzymy kontener tetromino
                            Entity container = ecb.CreateEntity();
                            ecb.AddComponent(container, new TetrominoContainer());
                            ecb.AddComponent(container, new FallingTetromino());
                            ecb.AddComponent(container, new FallSpeed { Value = 2f });
                            ecb.AddComponent(container, new PlayerControl { PlayerId = playerId });
                            ecb.AddComponent(container, new LocalTransform
                            {
                                Position = spawnPosition,
                                Rotation = quaternion.identity,
                                Scale = 1f
                            });
                            ecb.AddComponent(container, new LocalToWorld());

                            // Dodajemy bufor offsetów – tworzymy kwadrat z offsetami: (0,0), (1,0), (0,1), (1,1)
                            DynamicBuffer<TetrominoBlock> blockBuffer = ecb.AddBuffer<TetrominoBlock>(container);
                            NativeList<int2> offsets = new NativeList<int2>(Allocator.Temp);
                            offsets.Add(new int2(0, 0));
                            offsets.Add(new int2(1, 0));
                            offsets.Add(new int2(0, 1));
                            offsets.Add(new int2(1, 1));

                            // Dla każdego offsetu zapisujemy do bufora i tworzymy wizualny blok
                            for (int i = 0; i < offsets.Length; i++)
                            {
                                int2 offset = offsets[i];
                                blockBuffer.Add(new TetrominoBlock { Offset = offset });
                                Entity blockEntity = ecb.Instantiate(prefabElement.Value);
                                ecb.AddComponent(blockEntity, new TetrominoBlockLink { Container = container, BlockIndex = i });
                                ecb.AddComponent(blockEntity, new PlayerControl { PlayerId = playerId });
                                ecb.SetComponent(blockEntity, new LocalTransform
                                {
                                    Position = spawnPosition + new float3(offset.x, offset.y, 0f),
                                    Rotation = quaternion.identity,
                                    Scale = 1f
                                });
                            }
                            offsets.Dispose();
                            Debug.Log($"Tetromino spawned for player {playerId} at {spawnPosition}");
                        }
                        ecb.Playback(entityManager);
                    }
                }
                else
                {
                    Debug.LogWarning("PrefabElement singleton not found!");
                }
            }

            spawnPoints.Dispose();

            // Aktualizacja pozycji wizualnych bloków względem kontenera
            var containerTransformsLookup = GetComponentLookup<LocalTransform>(true);
            var containerBlockBuffers = GetBufferLookup<TetrominoBlock>(true);
            NativeParallelHashMap<Entity, LocalTransform> parentTransformMap =
                new NativeParallelHashMap<Entity, LocalTransform>(100, Allocator.TempJob);

            Entities
                .WithReadOnly(containerTransformsLookup)
                .WithAll<TetrominoContainer>()
                .ForEach((Entity entity) =>
                {
                    parentTransformMap.TryAdd(entity, containerTransformsLookup[entity]);
                }).Run();

            Dependency = Entities
                .WithReadOnly(parentTransformMap)
                .WithReadOnly(containerBlockBuffers)
                .WithBurst()
                .ForEach((ref LocalTransform blockLocalTransform, in TetrominoBlockLink link) =>
                {
                    if (parentTransformMap.TryGetValue(link.Container, out LocalTransform parentTransform))
                    {
                        DynamicBuffer<TetrominoBlock> blockBuffer = containerBlockBuffers[link.Container];
                        if (link.BlockIndex >= 0 && link.BlockIndex < blockBuffer.Length)
                        {
                            int2 offset = blockBuffer[link.BlockIndex].Offset;
                            blockLocalTransform.Position = parentTransform.Position + new float3(offset.x, offset.y, 0f);
                        }
                    }
                }).ScheduleParallel(Dependency);

            Dependency.Complete();
            parentTransformMap.Dispose();
        }
    }
}
