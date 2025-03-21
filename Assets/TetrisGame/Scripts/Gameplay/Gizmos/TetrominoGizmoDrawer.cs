using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class TetrominoGizmoDrawer : MonoBehaviour
{
    private void OnDrawGizmos()
    {
        // Aby rysować w Scene View, sprawdzamy czy gra jest uruchomiona.
        if (!Application.isPlaying) return;
        
        var world = World.DefaultGameObjectInjectionWorld;
        if (world == null) return;
        
        var entityManager = world.EntityManager;
        // Zapytanie o encje, które mają LocalTransform oraz dynamiczny bufor TetrominoBlock.
        var query = entityManager.CreateEntityQuery(typeof(LocalTransform), typeof(GameLogic.TetrominoBlock));
        var entities = query.ToEntityArray(Unity.Collections.Allocator.Temp);
        
        foreach (var entity in entities)
        {
            // Pobieramy transform tetromino
            var localTransform = entityManager.GetComponentData<LocalTransform>(entity);
            // Pobieramy bufor bloków tetromino
            var buffer = entityManager.GetBuffer<GameLogic.TetrominoBlock>(entity);
            
            // Rysujemy każdy blok w odpowiedniej pozycji (transform + offset)
            foreach (var block in buffer)
            {
                Vector3 blockPos = new Vector3(
                    localTransform.Position.x + block.Offset.x,
                    localTransform.Position.y + block.Offset.y,
                    localTransform.Position.z);
                Gizmos.color = Color.green;
                // Rysujemy sześcian o wielkości nieco mniejszej niż komórka, by lepiej widzieć odstępy
                Gizmos.DrawCube(blockPos, Vector3.one * 0.95f);
            }
        }
        entities.Dispose();
    }
}