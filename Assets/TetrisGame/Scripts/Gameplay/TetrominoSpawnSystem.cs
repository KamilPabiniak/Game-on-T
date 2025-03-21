using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    public struct TetrominoPrefabTag : IComponentData { }
    
    public struct TetrominoBlock : IBufferElementData
    {
        public int2 Offset;
    }
    
    public struct TetrominoPlaced : IComponentData { }
    
   [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class TetrominoSpawnSystem : SystemBase
    {
        public float SpawnInterval = 3f;
        private float _timeSinceLastSpawn;

        protected override void OnUpdate()
        {
            float deltaTime = SystemAPI.Time.DeltaTime;
            _timeSinceLastSpawn += deltaTime;

            if (_timeSinceLastSpawn < SpawnInterval)
                return;

            _timeSinceLastSpawn = 0f;
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            
            if (SystemAPI.TryGetSingleton<PrefabElement>(out var prefabElement))
            {
                Entity tetrominoContainer = entityManager.CreateEntity(
                    typeof(LocalTransform), 
                    typeof(LocalToWorld)
                );
                
                entityManager.SetComponentData(tetrominoContainer, new LocalTransform
                {
                    Position = new float3(-6f, 30f, 0f),
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                
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
                
                for (int i = 0; i < offsets.Length; i++)
                {
                    Entity blockEntity = entityManager.Instantiate(prefabElement.Value);
                    
                    entityManager.AddComponentData(blockEntity, new Parent { Value = tetrominoContainer });
                    
                    entityManager.SetComponentData(blockEntity, new LocalTransform
                    {
                        Position = new float3(offsets[i].x, offsets[i].y, 0f),
                        Rotation = quaternion.identity,
                        Scale = 1f
                    });
                }
                offsets.Dispose();
                
                entityManager.AddComponentData(tetrominoContainer, new FallingTetromino());
                entityManager.AddComponentData(tetrominoContainer, new FallSpeed { Value = 2f });

                Debug.Log("Tetromino spawned with container: " + tetrominoContainer.Index);
            }
            else
            {
                Debug.LogWarning("Nie znaleziono singletonu PrefabElement!");
            }
        }
    }
}
