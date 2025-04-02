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
    /// A dictionary of all Home Assistant entities. The key is the entityID and the value is the HassState object.
    /// </summary>
    private static readonly Dictionary<string, HassState> HassStatesDict = new();

    /// <summary>
    /// Gets a dictionary of all Home Assistant entities. The key is the entityID and the value is the HassState object.
    /// </summary>
    /// <returns>A dictionary of HassState objects.</returns>
    public static Dictionary<string, HassState> GetHassStates()
    {
        return HassStatesDict;
    }
        
    /// <summary>
    /// Gets the HassState object from the specified entityID.
    /// </summary>
    /// <param name="entityID">The entityID of the HassState object to retrieve.</param>
    /// <returns>The HassState object associated with the entityID, or null if no entity is found.</returns>
    public static HassState GetHassState(string entityID)
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
        
        // Parse the response text into an array of HassState objects
        HassState[] hassStates = JsonHelper.ArrayFromJson<HassState>(responseText);
            
        // Iterate over the entities and handle each one
        foreach (HassState entity in hassStates)
        {
            if (string.IsNullOrEmpty(entity.attributes.friendly_name))
            {
                entity.attributes.friendly_name = entity.entity_id.Split(".")[1];
            }
            
            // Update or add the entity state
            if (HassStatesDict.TryGetValue(entity.entity_id, out HassState existingEntity))
            {
                existingEntity.state = entity.state;
                existingEntity.attributes = entity.attributes;
            }
            else
            {
                // Get the type from the entityID
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

    public static List<CalendarEvent> ConvertHassCalendarResponse(string calendarResponse)
    {
        CalendarEvent[] events = JsonHelper.ArrayFromJson<CalendarEvent>(calendarResponse);
        return events.ToList();
    }
}
    
[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class HassState
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

[Serializable]
[SuppressMessage("ReSharper", "InconsistentNaming")]
public class CalendarEvent
{
    public EventDate start;
    public EventDate end;
    public string summary;
    public string description;
    public string location;
    
    [Serializable]
    public class EventDate
    {
        public string date;
        public string dateTime;

        // Converts to a DateTime object for easier use
        public DateTime? GetDateTime()
        {
            if (!string.IsNullOrEmpty(dateTime))
                return DateTime.Parse(dateTime);
            if (!string.IsNullOrEmpty(date))
                return DateTime.Parse(date);
            return null;
        }
    }
}



