using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.NetCode;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace GameLogic
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct TetrominoSpawnerSystem : ISystem
    {
        private EntityQuery _spawnPointsQuery;
        private EntityQuery _playersQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PrefabElement>();
            _spawnPointsQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<TetrominoSpawnPoint>()
                .Build(ref state);
            _playersQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<NetworkId>()
                .WithNone<Tetromino>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var prefab = SystemAPI.GetSingleton<PrefabElement>().Value;
            var spawnPoints = _spawnPointsQuery.ToComponentDataArray<TetrominoSpawnPoint>(Allocator.Temp);
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (idEntity, netId) in SystemAPI.Query<Entity, NetworkId>())
            {
                if (spawnPoints.Length < 2)
                {
                    Debug.LogError("Not enough spawn points!");
                    continue;
                }

                var spawnIndex = netId.Value % 2;
                var spawnPos = spawnPoints[spawnIndex].Position;

                // Spawn new tetromino
                var tetromino = ecb.Instantiate(prefab);
                ecb.AddComponent(tetromino, new Tetromino
                {
                    PlayerId = netId.Value,
                    RotationPivot = float3.zero,
                    Color = (netId.Value == 0) ? 
                        new float4(0, 0, 1, 1) : // Blue for host
                        new float4(1, 0, 0, 1)   // Red for client
                });
                
                // Add ghost and network components
                ecb.AddComponent(tetromino, new GhostOwner { NetworkId = netId.Value });
                ecb.AddComponent(tetromino, new LocalTransform
                {
                    Position = spawnPos,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });

                // Add block offsets for I shape (przykładowy kształt)
                var buffer = ecb.AddBuffer<TetrominoOffset>(tetromino);
                buffer.Add(new TetrominoOffset { Value = new float2(-1, 0) });
                buffer.Add(new TetrominoOffset { Value = new float2(0, 0) });
                buffer.Add(new TetrominoOffset { Value = new float2(1, 0) });
                buffer.Add(new TetrominoOffset { Value = new float2(2, 0) });

                // Assign to player
                ecb.AddComponent(idEntity, new Tetromino { PlayerId = netId.Value });
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
            spawnPoints.Dispose();
        }
    }
}