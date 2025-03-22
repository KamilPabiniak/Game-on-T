using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    public struct TetrominoContainer : IComponentData { }

    public struct TetrominoBlock : IBufferElementData
    {
        public int2 Offset;
    }

    public struct TetrominoBlockLink : IComponentData
    {
        public Entity Container;
        public int BlockIndex;
    }

    public struct TetrominoPlaced : IComponentData { }
    public struct VisualSpawnedTag : IComponentData { }
    public struct VisualOffset : IComponentData
    {
        public float3 Value;
    }
    
    public struct LocalPlayerData : IComponentData
    { 
        public int PlayerId;
    }
    
    public struct PlayerControl : IComponentData
    {
        public int PlayerId;
    }

   [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial class TetrominoSpawnAndFollowSystem : SystemBase
    {
        public float SpawnInterval = 3f;
        private float _timeSinceLastSpawn;

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            _timeSinceLastSpawn += deltaTime;
            
            // Sprawdzamy czy są jakieś spawny ustawione w scenie
            EntityQuery spawnPointQuery = GetEntityQuery(ComponentType.ReadOnly<TetrominoSpawnPoint>());
            NativeArray<TetrominoSpawnPoint> spawnPoints = spawnPointQuery.ToComponentDataArray<TetrominoSpawnPoint>(Allocator.TempJob);

            // Jeśli nie ma żadnych punktów spawnu – nie robimy spawnów
            if (spawnPoints.Length == 0)
            {
                spawnPoints.Dispose();
                return;
            }
            
            // Sprawdzamy, czy dla któregoś gracza już leci figura – nie spawnujemy nowej, dopóki poprzednia nie została umieszczona
            EntityQuery fallingQuery = GetEntityQuery(ComponentType.ReadOnly<FallingTetromino>());
            if (_timeSinceLastSpawn >= SpawnInterval && fallingQuery.CalculateEntityCount() == 0)
            {
                _timeSinceLastSpawn = 0f;
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                
                // Pobieramy prefabrykat – musi być ustawiony w singletonie
                if (SystemAPI.TryGetSingleton<PrefabElement>(out var prefabElement))
                {
                    using (var ecb = new EntityCommandBuffer(Allocator.Temp))
                    {
                        // Dla dwóch graczy – host (PlayerId = 0) oraz klient (PlayerId = 1)
                        for (int playerId = 0; playerId < 2; playerId++)
                        {
                            // Wybieramy losowo spawn point dla danego gracza
                            NativeList<TetrominoSpawnPoint> pointsForPlayer = new NativeList<TetrominoSpawnPoint>(Allocator.Temp);
                            for (int i = 0; i < spawnPoints.Length; i++)
                            {
                                if (spawnPoints[i].PlayerId == playerId)
                                    pointsForPlayer.Add(spawnPoints[i]);
                            }
                            
                            if (pointsForPlayer.Length == 0)
                            {
                                pointsForPlayer.Dispose();
                                continue;
                            }
                            
                            int randomIndex = UnityEngine.Random.Range(0, pointsForPlayer.Length);
                            float3 spawnPosition = pointsForPlayer[randomIndex].Position;
                            pointsForPlayer.Dispose();
                            
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
                            
                            DynamicBuffer<TetrominoBlock> blockBuffer = ecb.AddBuffer<TetrominoBlock>(container);
                            
                            // Losowy kształt – kwadrat lub linia
                            NativeList<int2> offsets = new NativeList<int2>(Allocator.Temp);
                            int shapeIndex = UnityEngine.Random.Range(0, 2);
                            if (shapeIndex == 0)
                            {
                                // Kwadrat 2x2
                                offsets.Add(new int2(0, 0));
                                offsets.Add(new int2(1, 0));
                                offsets.Add(new int2(0, 1));
                                offsets.Add(new int2(1, 1));
                            }
                            else
                            {
                                // Linia 4x1
                                offsets.Add(new int2(0, 0));
                                offsets.Add(new int2(1, 0));
                                offsets.Add(new int2(2, 0));
                                offsets.Add(new int2(3, 0));
                            }
                            
                            // Tworzymy bloki i przypisujemy do nich link do kontenera
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
            
            // Aktualizacja pozycji bloków oparta o kontener pozostaje bez zmian
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
