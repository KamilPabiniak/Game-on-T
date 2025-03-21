using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class TetrominoGizmoDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;
        
        var entityManager = world.EntityManager;
        var query = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(GameLogic.TetrominoBlock));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        
        foreach (var entity in entities)
        {
            var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            var buffer = entityManager.GetBuffer<GameLogic.TetrominoBlock>(entity);
            
            foreach (var block in buffer)
            {
                Vector3 blockPos = new Vector3(
                    localTransform.Position.x + block.Offset.x,
                    localTransform.Position.y + block.Offset.y,
                    localTransform.Position.z);
                Gizmos.color = Color.green;
                Gizmos.DrawCube(blockPos, Vector3.one * 0.95f);
            }
        }
        entities.Dispose();
    }
}