using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


namespace GameLogic
{
    public struct TetrominoContainer : IComponentData { }
    
    public struct TetrominoBlock : IBufferElementData
    {
        public int2 Offset;
    }
    public struct TetrominoPlaced : IComponentData { }
    
    public struct LocalToParent : IComponentData { }
    

    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoSpawnSystem : SystemBase
    {
        public float SpawnInterval = 3f;
        private float _timeSinceLastSpawn;

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            _timeSinceLastSpawn += deltaTime;
            
            EntityQuery fallingQuery = GetEntityQuery(ComponentType.ReadOnly<FallingTetromino>());
            if (fallingQuery.CalculateEntityCount() > 0)
                return;

            if (_timeSinceLastSpawn < SpawnInterval)
                return;

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
                    ecb.AddComponent(container, new LocalTransform
                    {
                        Position = new float3(5f, 18f, 0f),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                    ecb.AddComponent(container, new LocalToWorld());
                    
                    var blockBuffer = ecb.AddBuffer<TetrominoBlock>(container);
                    
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
                    
                    for (int i = 0; i < offsets.Length; i++)
                    {
                        blockBuffer.Add(new TetrominoBlock { Offset = offsets[i] });
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
