using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Managers;
using Proyecto26;
using UnityEngine;

public static class HassStates
{
    /// <summary>
    /// A dictionary of all Home Assistant entities. The key is the entity ID and the value is the HassEntity object.
    /// </summary>
    private static readonly Dictionary<string, HassEntity> HassStatesDict = new();

    /// <summary>
    /// Gets a dictionary of all Home Assistant entities. The key is the entity ID and the value is the HassEntity object.
    /// </summary>
    /// <returns>A dictionary of HassEntity objects.</returns>
    public static Dictionary<string, HassEntity> GetHassStates()
    {
        return HassStatesDict;
    }
        
    /// <summary>
    /// Gets the HassEntity object from the specified entity ID.
    /// </summary>
    /// <param name="entityID">The entity ID of the HassEntity object to retrieve.</param>
    /// <returns>The HassEntity object associated with the entity ID, or null if no entity is found.</returns>
    public static HassEntity GetHassState(string entityID)
    {
        return string.IsNullOrEmpty(entityID) ? null : HassStatesDict.GetValueOrDefault(entityID);
    }

    /// <summary>
    /// Handles the response from Home Assistant and updates the state entities.
    /// </summary>
    /// <param name="responseText">The response text from Home Assistant containing entity states.</param>
    public static void OnHassStatesResponse(string responseText)
    {
        
        if (GameManager.Instance.DebugLogGetHassEntities)
            Debug.Log(responseText);
        
        // Parse the response text into an array of HassEntity objects
        HassEntity[] hassEntities = JsonHelper.ArrayFromJson<HassEntity>(responseText);
            
        // Iterate over the entities and handle each one
        foreach (HassEntity entity in hassEntities)
        {
            // Update or add the entity state
            if (HassStatesDict.TryGetValue(entity.entity_id, out HassEntity existingEntity))
            {
                existingEntity.state = entity.state;
                existingEntity.attributes = entity.attributes;
            }
            else
            {
                // Get the type from the entity ID
                string type = entity.entity_id.Split('.')[0];

                // Try to parse the type as an EDeviceType and set the device type if it was parsed successfully
                if (Enum.TryParse(type, true, out EDeviceType deviceType))
                    entity.DeviceType = deviceType;

                // Update or add the entity
                HassStatesDict[entity.entity_id] = entity;
            }
            
        }
            
        // Invoke the event that the Hass states have changed
        EventManager.InvokeOnHassStatesChanged();
            
        // Copy the entities to the inspector field for debugging
        GameManager.Instance.InspectorHassStates = HassStatesDict.Values.ToArray();
    }

    public static void OnHassConfigResponse(string responseText)
    {
        HassConfig config = JsonUtility.FromJson<HassConfig>(responseText);
        GameManager.Instance.OnHassConfigLoaded(config);
    }
    
    public static List<WeatherForecast> ConvertHassWeatherResponse(string responseText)
    {
        Match match = Regex.Match(responseText, @"""forecast"":(\[.*?\])", RegexOptions.Singleline);
        return match.Success ? JsonHelper.ArrayFromJson<WeatherForecast>(match.Groups[1].Value)?.ToList() : null;
    }
}
    
[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class HassEntity
{
    public string entity_id;
    public string state = "";
    public HassEntityAttributes attributes;
    public EDeviceType DeviceType;

    [Serializable]
    public class HassEntityAttributes
    {
        public string unit_of_measurement;
        public string device_class;
        public int min_color_temp_kelvin;
        public int max_color_temp_kelvin;
        public string[] supported_color_modes;
        public string color_mode;
        public int brightness;
        public int color_temp_kelvin;
        public int color_temp;
        public int[] rgb_color;
        public float[] hs_color;
        public string icon;
        public string friendly_name;
        public float current_temperature;
        public float temperature;
        public string temperature_unit;
    }
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class HassConfig
{
    //public List<string> components;
    public string country;
    public string currency;
    //public int elevation;
    public string language;
    //public double latitude;
    //public string location_name;
    //public double longitude;
    //public int radius;
    public string time_zone;
    public UnitSystem unit_system;
    public string version;
    
    [Serializable]
    public class UnitSystem
    {
        public string length;
        public string accumulated_precipitation;
        public string area;
        public string mass;
        public string pressure;
        public string temperature;
        public string volume;
        public string wind_speed;
    }
}

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class WeatherForecast
{
    public string condition;
    public double precipitation_probability;
    public string datetime;
    public double wind_bearing;
    public double uv_index;
    public double temperature;
    public double templow;
    public double wind_gust_speed;
    public double wind_speed;
    public double precipitation;
    public int humidity;
}

    


