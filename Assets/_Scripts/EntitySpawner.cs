using System;
using Managers;
using Structs;
using UnityEngine;
using Utils;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private Entity EntityPrefab;
    [SerializeField] private Transform HassUITranslation;

    /// <summary>
    /// Spawns a new entity at the specified transform position and rotation.
    /// </summary>
    /// <param name="position">The transform at which the entity will be spawned.</param>
    /// <param name="entityObject"></param>
    public void SpawnSavedEntity(EntityObject entityObject)
    {
        // Instantiate the entity at the given position and rotation
        Entity newEntity = Instantiate(EntityPrefab);
        
        newEntity.transform.localPosition = entityObject.Position;
        
        // Set the entity ID to the new entity
        newEntity.SetEntityObject(entityObject);
    }

    /// <summary>
    /// Spawns a new entity at the specified transform position and rotation.
    /// </summary>
    /// <param name="selectedEntityID">The ID of the entity to be assigned.</param>
    /// <param name="position">The transform at which the entity will be spawned.</param>
    public void SpawnNewEntity(string selectedEntityID, Vector3 position)
    {
        // Instantiate the entity at the given position and rotation
        Entity newEntity = Instantiate(EntityPrefab, HassUITranslation, false);
        
        // Slightly offset the position of the new entity
        Vector3 newPosition = position - newEntity.transform.forward * 0.1f;

        string id = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
        EntityObject entityObject = new (id, selectedEntityID, newPosition);
        GameManager.Instance.EntityObjects.Add(entityObject);
        SaveFile.SaveEntityObjects();
        
        newEntity.transform.localPosition = entityObject.Position;
        
        // Set the entity ID to the new entity
        newEntity.SetEntityObject(entityObject);
    }
}