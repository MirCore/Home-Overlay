using System;
using Managers;
using Structs;
using UnityEngine;

/// <summary>
/// Handles the spawning of panels based on predefined prefabs and panel data.
/// </summary>
public class PanelSpawner : MonoBehaviour
{
    /// <summary>
    /// Prefab for the button panel.
    /// </summary>
    [SerializeField] private Panels.Panel PanelButtonPrefab;

    /// <summary>
    /// Prefab for the sensor panel.
    /// </summary>
    [SerializeField] private Panels.Panel PanelSensorPrefab;

    /// <summary>
    /// Prefab for the weather panel.
    /// </summary>
    [SerializeField] private Panels.Panel PanelWeatherPrefab;

    /// <summary>
    /// Prefab for the calendar panel.
    /// </summary>
    [SerializeField] private Panels.Panel PanelCalendarPrefab;
    
    [SerializeField] private DemoPanelsManager DemoPanelPrefab;

    /// <summary>
    /// Spawns a saved panel using the provided panel data.
    /// </summary>
    /// <param name="panelData">The data for the panel to be spawned.</param>
    public void SpawnSavedPanel(PanelData panelData)
    {
        Panels.Panel prefab = GetPanelPrefab(panelData.EntityID);
        Panels.Panel newPanel = Instantiate(prefab);
        
        // Initialize the panel with the provided data
        newPanel.InitPanel(panelData);
    }
    
    /// <summary>
    /// Spawns a new panel at the specified position.
    /// </summary>
    /// <param name="selectedEntityID">The ID of the panel to be assigned.</param>
    /// <param name="position">The position at which the panel will be spawned.</param>
    public void SpawnNewPanel(string selectedEntityID, Vector3 position)
    {
        Panels.Panel prefab = GetPanelPrefab(selectedEntityID);
        Panels.Panel newPanel = Instantiate(prefab);
        
        // Generate a unique ID for the new panel
        string id = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
        // Create new PanelData
        PanelData panelData = new (id, selectedEntityID, position);
        // Add the new panel data to the manager
        PanelManager.Instance.AddNewPanelData(panelData);
        
        // Initialize the panel with the new data
        newPanel.InitPanel(panelData);
    }

    /// <summary>
    /// Gets the appropriate panel prefab based on the selected entity ID.
    /// </summary>
    /// <param name="selectedEntityID">The ID of the entity to determine the prefab.</param>
    /// <returns>The panel prefab corresponding to the entity ID.</returns>
    private Panels.Panel GetPanelPrefab(string selectedEntityID)
    {       
        // Extract the type from the entityID
        string type = "";
        if (!string.IsNullOrEmpty(selectedEntityID))
            type = selectedEntityID.Split('.')[0];
        
                
        // Try to parse the type as an EDeviceType and return the corresponding prefab
        return Enum.TryParse(type, true, out EDeviceType deviceType) ? deviceType switch
        {
            EDeviceType.LIGHT or EDeviceType.SWITCH => PanelButtonPrefab,
            EDeviceType.WEATHER => PanelWeatherPrefab,
            EDeviceType.CALENDAR => PanelCalendarPrefab,
            _ => PanelSensorPrefab // Default fallback
        } : PanelSensorPrefab; // Default fallback if parsing fails
    }

    /// <summary>
    /// Spawns demo panels at the specified transform position and rotation.
    /// </summary>
    /// <param name="pos">The transform specifying the position and rotation for the demo panels.</param>
    public void SpawnDemoPanels(Transform pos)
    {
        DemoPanelsManager demoPanelsManager = Instantiate(DemoPanelPrefab);
        demoPanelsManager.transform.position = pos.position;
        demoPanelsManager.transform.rotation = pos.rotation;
    }
}