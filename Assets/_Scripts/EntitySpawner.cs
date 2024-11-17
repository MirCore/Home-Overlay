using UnityEngine;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private Entity EntityPrefab;
    
    /// <summary>
    /// Spawns a new entity at the specified transform position and rotation.
    /// </summary>
    /// <param name="selectedEntityID">The ID of the entity to be assigned.</param>
    /// <param name="spawnTransform">The transform at which the entity will be spawned.</param>
    public void SpawnEntity(string selectedEntityID, Transform spawnTransform)
    {
        // Instantiate the entity at the given position and rotation
        Entity newEntity = Instantiate(EntityPrefab, spawnTransform.position, spawnTransform.rotation);
        
        // Slightly offset the position of the new entity
        newEntity.transform.position -= spawnTransform.forward * 0.1f;
        
        // Set the entity ID to the new entity
        newEntity.SetEntityID(selectedEntityID);
    }
}