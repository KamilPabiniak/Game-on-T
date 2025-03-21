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

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial class TetrominoSpawnAndFollowSystem : SystemBase
    {
        public float SpawnInterval = 3f;
        private float _timeSinceLastSpawn;

        protected override void OnUpdate()
        {
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
            
            float deltaTime = SystemAPI.Time.DeltaTime;
            _timeSinceLastSpawn += deltaTime;

            EntityQuery fallingQuery = GetEntityQuery(ComponentType.ReadOnly<FallingTetromino>());
            if (fallingQuery.CalculateEntityCount() == 0 && _timeSinceLastSpawn >= SpawnInterval)
            {
                _timeSinceLastSpawn = 0f;
                var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

                if (SystemAPI.TryGetSingleton<PrefabElement>(out var prefabElement))
                {
                    using (var ecb = new EntityCommandBuffer(Allocator.Temp))
                    {
                        Entity container = ecb.CreateEntity();
                        ecb.AddComponent(container, new TetrominoContainer());
                        ecb.AddComponent(container, new FallingTetromino());
                        ecb.AddComponent(container, new FallSpeed { Value = 2f });
                        
                        float3 containerStartPos = new float3(5f, 18f, 0f);
                        ecb.AddComponent(container, new LocalTransform
                        {
                            Position = containerStartPos,
                            Rotation = quaternion.identity,
                            Scale = 1f
                        });
                        ecb.AddComponent(container, new LocalToWorld());
                        
                        DynamicBuffer<TetrominoBlock> blockBuffer = ecb.AddBuffer<TetrominoBlock>(container);
                        
                        NativeList<int2> offsets = new NativeList<int2>(Allocator.Temp);
                        int shapeIndex = UnityEngine.Random.Range(0, 2);
                        if (shapeIndex == 0)
                        {
                            //square   2x2
                            offsets.Add(new int2(0, 0));
                            offsets.Add(new int2(1, 0));
                            offsets.Add(new int2(0, 1));
                            offsets.Add(new int2(1, 1));
                        }
                        else
                        {
                            // line 4x1
                            offsets.Add(new int2(0, 0));
                            offsets.Add(new int2(1, 0));
                            offsets.Add(new int2(2, 0));
                            offsets.Add(new int2(3, 0));
                        }
                        
                        for (int i = 0; i < offsets.Length; i++)
                        {
                            int2 offset = offsets[i];
                            blockBuffer.Add(new TetrominoBlock { Offset = offset });
                            Entity blockEntity = ecb.Instantiate(prefabElement.Value);
                            ecb.AddComponent(blockEntity, new TetrominoBlockLink { Container = container, BlockIndex = i });
                            ecb.SetComponent(blockEntity, new LocalTransform
                            {
                                Position = containerStartPos + new float3(offset.x, offset.y, 0f),
                                Rotation = quaternion.identity,
                                Scale = 1f
                            });
                        }
                        offsets.Dispose();

                        Debug.Log($"Tetromino spawned with container Entity {container.Index}");
                        ecb.Playback(entityManager);
                    }
                }
                else
                {
                    Debug.LogWarning("PrefabElement singleton not found!");
                }
            }
        }
    }
}
