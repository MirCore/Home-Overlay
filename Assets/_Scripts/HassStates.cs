using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Managers;
using Proyecto26;
using Structs;
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
    /// The configuration data of Home Assistant.
    /// </summary>
    private static HassConfig _hassConfig;
    
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
#if UNITY_EDITOR
        if (GameManager.Instance.DebugLogGetHassEntities)
            Debug.Log(responseText);
#endif
        
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
                // Try to parse the type as an EDeviceType and set the device type if it was parsed successfully
                entity.DeviceType = GetDeviceType(entity.entity_id);

                // Update or add the entity
                HassStatesDict[entity.entity_id] = entity;
            }
        }
            
        // Invoke the event that the Hass states have changed
        EventManager.InvokeOnHassStatesChanged();
            
#if UNITY_EDITOR
        // Copy the entities to the inspector field for debugging
        GameManager.Instance.InspectorHassStates = HassStatesDict.Values.ToArray();
#endif
    }

    /// <summary>
    /// Retrieves the device type for a given entity ID based on its prefix.
    /// </summary>
    /// <param name="entityID">The entity ID whose device type is to be determined.</param>
    /// <returns>The device type as an EDeviceType enumeration.</returns>
    public static EDeviceType GetDeviceType(string entityID)
    {
        // Get the type from the entityID
        string type = entityID.Split('.')[0];

        Enum.TryParse(type, true, out EDeviceType deviceType);
            
        return deviceType;
    }

    /// <summary>
    /// Handles the response from the Home Assistant configuration endpoint and updates the internal configuration data.
    /// </summary>
    /// <param name="responseText">The JSON response containing the configuration data from Home Assistant.</param>
    public static void OnHassConfigResponse(string responseText)
    {
        _hassConfig = JsonUtility.FromJson<HassConfig>(responseText);
    }

    /// <summary>
    /// Converts the response text from the Home Assistant weather service into a list of WeatherForecast objects.
    /// </summary>
    /// <param name="responseText">The JSON response string containing weather forecast data.</param>
    /// <returns>A list of WeatherForecast objects if parsing is successful; otherwise, null.</returns>
    public static List<WeatherForecast> ConvertHassWeatherResponse(string responseText)
    {
        Match match = Regex.Match(responseText, @"""forecast"":(\[.*?\])", RegexOptions.Singleline);
        return match.Success ? JsonHelper.ArrayFromJson<WeatherForecast>(match.Groups[1].Value)?.ToList() : null;
    }

    /// <summary>
    /// Converts a JSON string containing calendar event data returned from Home Assistant into a list of CalendarEvent objects.
    /// </summary>
    /// <param name="calendarResponse">The JSON string containing the calendar events.</param>
    /// <returns>A list of CalendarEvent objects derived from the parsed JSON.</returns>
    public static List<CalendarEvent> ConvertHassCalendarResponse(string calendarResponse)
    {
        CalendarEvent[] events = JsonHelper.ArrayFromJson<CalendarEvent>(calendarResponse);
        return events.ToList();
    }

    /// <summary>
    /// Retrieves the configuration data for Home Assistant.
    /// </summary>
    /// <returns>A HassConfig object containing Home Assistant configuration details.</returns>
    public static HassConfig GetHassConfig()
    {
        return _hassConfig;
    }
}
    