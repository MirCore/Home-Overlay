using System;
using Managers;
using Structs;
using UnityEngine;
using Utils;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private Entity EntityButtonPrefab;
    [SerializeField] private Entity EntitySensorPrefab;
    [SerializeField] private Transform HassUITranslation;

    /// <summary>
    /// Spawns a new entity at the specified transform position and rotation.
    /// </summary>
    /// <param name="entityObject"></param>
    public void SpawnSavedEntity(EntityObject entityObject)
    {
        // Get the type from the entity ID
        string type = entityObject.EntityID.Split('.')[0];
                
        // Try to parse the type as an EDeviceType
        Enum.TryParse(type, true, out EDeviceType deviceType);

        // Instantiate the entity at the given position and rotation
        Entity newEntity;
        if (deviceType is EDeviceType.LIGHT or EDeviceType.SWITCH)
        {
            newEntity = Instantiate(EntityButtonPrefab, entityObject.Position, Quaternion.identity);
        }
        else
        {
            newEntity = Instantiate(EntitySensorPrefab, entityObject.Position, Quaternion.identity);
        }
        
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
        // Get the type from the entity ID
        string type = selectedEntityID.Split('.')[0];
                
        // Try to parse the type as an EDeviceType
        Enum.TryParse(type, true, out EDeviceType deviceType);

        // Instantiate the entity at the given position and rotation
        Entity newEntity;
        if (deviceType is EDeviceType.LIGHT or EDeviceType.SWITCH)
        {
            newEntity = Instantiate(EntityButtonPrefab, HassUITranslation, false);
        }
        else
        {
            newEntity = Instantiate(EntitySensorPrefab, HassUITranslation, false);
        }

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