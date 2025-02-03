using System;
using Managers;
using Structs;
using UnityEngine;
using Utils;

public class EntitySpawner : MonoBehaviour
{
    [SerializeField] private Entity.Entity EntityButtonPrefab;
    [SerializeField] private Entity.Entity EntitySensorPrefab;
    [SerializeField] private Entity.Entity EntityWeatherPrefab;
    [SerializeField] private Entity.Entity EntityCalendarPrefab;
    [SerializeField] private Transform HassUITranslation;

    /// <summary>
    /// Spawns a new entity at the specified transform position and rotation.
    /// </summary>
    /// <param name="entityObject"></param>
    public void SpawnSavedEntity(EntityObject entityObject)
    {
        Entity.Entity prefab = GetEntityPrefab(entityObject.EntityID);
        Entity.Entity newEntity = Instantiate(prefab, entityObject.Position, Quaternion.identity);
        
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
        Entity.Entity prefab = GetEntityPrefab(selectedEntityID);
        Entity.Entity newEntity = Instantiate(prefab, HassUITranslation, false);

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

    private Entity.Entity GetEntityPrefab(string selectedEntityID)
    {
        // Get the type from the entity ID
        string type = selectedEntityID.Split('.')[0];
                
        // Try to parse the type as an EDeviceType and return the corresponding prefab
        return Enum.TryParse(type, true, out EDeviceType deviceType) ? deviceType switch
        {
            EDeviceType.LIGHT or EDeviceType.SWITCH => EntityButtonPrefab,
            EDeviceType.WEATHER => EntityWeatherPrefab,
            EDeviceType.CALENDAR => EntityCalendarPrefab,
            _ => EntitySensorPrefab
        } : EntitySensorPrefab; // Default fallback
    }
}