using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace TetrisGame.Scripts.Gameplay
{
    public struct Tetrimino : IComponentData
    {
        public int Type; 
    }

    public struct Player : IComponentData
    {
        public int PlayerId;
    }
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial class TetriminoSpawnerSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            bool hasActiveTetrimino = false;
            Entities.WithAll<Tetrimino>().ForEach((in Tetrimino t) =>
            {
                hasActiveTetrimino = true;
            }).Run();

            if (!hasActiveTetrimino)
            {
                var entityManager = World.EntityManager;
                Entity tetriminoEntity = entityManager.CreateEntity(
                    typeof(Tetrimino),
                    typeof(LocalTransform),
                    typeof(PhysicsVelocity),
                    typeof(PhysicsCollider),
                    typeof(Player)
                );

             
                int tetriminoType = UnityEngine.Random.Range(0, 2);
                entityManager.SetComponentData(tetriminoEntity, new Tetrimino { Type = tetriminoType });
                
                entityManager.SetComponentData(tetriminoEntity, new LocalTransform {Position = new float3(0, 10, 0)});
                
                entityManager.SetComponentData(tetriminoEntity, new PhysicsVelocity { Linear = new float3(0, -1, 0) });
                
                entityManager.SetComponentData(tetriminoEntity, new Player { PlayerId = 0 });

                BlobAssetReference<Collider> collider = BoxCollider.Create(
                    new BoxGeometry
                    {
                        Center = float3.zero,
                        Size = new float3(1, 1, 1),
                        Orientation = quaternion.identity
                    },
                    CollisionFilter.Default
                );
                entityManager.SetComponentData(tetriminoEntity, new PhysicsCollider { Value = collider });
            }
        }
    }
}
