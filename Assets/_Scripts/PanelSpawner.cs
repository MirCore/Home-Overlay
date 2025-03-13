using System;
using Managers;
using Structs;
using UnityEngine;

public class PanelSpawner : MonoBehaviour
{
    [SerializeField] private Panel.Panel PanelButtonPrefab;
    [SerializeField] private Panel.Panel PanelSensorPrefab;
    [SerializeField] private Panel.Panel PanelWeatherPrefab;
    [SerializeField] private Panel.Panel PanelCalendarPrefab;
    [SerializeField] private Transform HassUITranslation;


    public void SpawnSavedPanel(PanelData panelData)
    {
        Panel.Panel prefab = GetEntityPrefab(panelData.EntityID);
        Panel.Panel newPanel = Instantiate(prefab);
        
        // Set the panel ID to the new panel
        newPanel.InitPanel(panelData);
    }

    /// <summary>
    /// Spawns a new panel at the specified transform position and rotation.
    /// </summary>
    /// <param name="selectedEntityID">The ID of the panel to be assigned.</param>
    /// <param name="position">The transform at which the panel will be spawned.</param>
    public void SpawnNewEntity(string selectedEntityID, Vector3 position)
    {
        Panel.Panel prefab = GetEntityPrefab(selectedEntityID);
        Panel.Panel newPanel = Instantiate(prefab, HassUITranslation, false);

        // Slightly offset the position of the new panel
        Vector3 newPosition = position - newPanel.transform.forward * 0.1f;

        string id = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
        PanelData panelData = new (id, selectedEntityID, newPosition);
        PanelManager.Instance.AddNewEntity(panelData);
        
        // Set the panel ID to the new panel
        newPanel.InitPanel(panelData);
    }

    private Panel.Panel GetEntityPrefab(string selectedEntityID)
    {
        // Get the type from the panel ID
        string type = selectedEntityID.Split('.')[0];
                
        // Try to parse the type as an EDeviceType and return the corresponding prefab
        return Enum.TryParse(type, true, out EDeviceType deviceType) ? deviceType switch
        {
            EDeviceType.LIGHT or EDeviceType.SWITCH => PanelButtonPrefab,
            EDeviceType.WEATHER => PanelWeatherPrefab,
            EDeviceType.CALENDAR => PanelCalendarPrefab,
            _ => PanelSensorPrefab
        } : PanelSensorPrefab; // Default fallback
    }

}