using System;
using Managers;
using Structs;
using UnityEngine;

public class PanelSpawner : MonoBehaviour
{
    [SerializeField] private Panels.Panel PanelButtonPrefab;
    [SerializeField] private Panels.Panel PanelSensorPrefab;
    [SerializeField] private Panels.Panel PanelWeatherPrefab;
    [SerializeField] private Panels.Panel PanelCalendarPrefab;

    public void SpawnSavedPanel(PanelData panelData)
    {
        Panels.Panel prefab = GetPanelPrefab(panelData.EntityID);
        Panels.Panel newPanel = Instantiate(prefab);
        
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
        Panels.Panel prefab = GetPanelPrefab(selectedEntityID);
        Panels.Panel newPanel = Instantiate(prefab);

        // Slightly offset the position of the new panel
        Vector3 newPosition = position - newPanel.transform.forward * 0.1f;

        string id = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
        PanelData panelData = new (id, selectedEntityID, newPosition);
        PanelManager.Instance.AddNewPanelData(panelData);
        
        // Set the panel ID to the new panel
        newPanel.InitPanel(panelData);
    }

    private Panels.Panel GetPanelPrefab(string selectedEntityID)
    {
        string type = "";
        // Get the type from the entityID
        if (!string.IsNullOrEmpty(selectedEntityID))
            type = selectedEntityID.Split('.')[0];
        
                
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