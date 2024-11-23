using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Proyecto26;
using UnityEngine;

public static class HassStates
{
    private static readonly Dictionary<string, HassEntity> HassStatesDict = new();
        

    public static Dictionary<string, HassEntity> GetHassStates()
    {
        return HassStatesDict;
    }
        
    public static HassEntity GetHassState(string entityID)
    {
        if (string.IsNullOrEmpty(entityID))
            return null;

        HassStatesDict.TryGetValue(entityID, out HassEntity state);
            
        return state;
    }

    public static void UpdateState(HassEntity entity)
    {
        HassStatesDict[entity.entity_id] = entity;
    }

    /// <summary>
    /// Handles the response from Home Assistant and updates the state entities.
    /// </summary>
    /// <param name="responseText">The response text from Home Assistant containing entity states.</param>
    public static void OnHassStatesResponse(string responseText)
    {
        Debug.Log(responseText);
            
        // Parse the response text into an array of HassEntity objects
        HassEntity[] hassEntities = JsonHelper.ArrayFromJson<HassEntity>(responseText);
            
        // Iterate over the entities and handle each one
        foreach (HassEntity entity in hassEntities)
        {
            // Get the type from the entity ID
            string type = entity.entity_id.Split('.')[0].ToUpper();
                
            // Try to parse the type as an EDeviceType
            if (Enum.TryParse(type, out EDeviceType deviceType))
            {
                // Set the device type if it was parsed successfully
                entity.DeviceType = deviceType;
            }
                
            // Update or add the entity
            HassStatesDict[entity.entity_id] = entity;
        }
            
        // Invoke the event that the Hass states have changed
        EventManager.InvokeOnHassStatesChanged();
            
        // Copy the entities to the inspector field for debugging
        GameManager.Instance.InspectorHassStates = HassStatesDict.Values.ToArray();
    }
}
    
[Serializable]
public class HassEntity
{
    public string entity_id;
    public string state;
    public HassEntityAttributes attributes;
    public EDeviceType DeviceType;
}

[Serializable]
public class HassEntityAttributes
{
    public string friendly_name;
    public int[] rgb_color;
    public float[] hs_color;
    public string icon;
    public int brightness;
}