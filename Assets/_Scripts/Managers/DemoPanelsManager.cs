using System;
using Panels;
using Structs;
using UnityEngine;

namespace Managers
{
    /// <summary>
    /// Initializes demo panels in the scene.
    /// </summary>
    public class DemoPanelsManager : MonoBehaviour
    {
        /// <summary>
        /// Reference to button panel.
        /// </summary>
        [SerializeField] private Panel ButtonPanel;
        
        /// <summary>
        /// Reference to calendar panel.
        /// </summary>
        [SerializeField] private Panel CalendarPanel;
        
        /// <summary>
        /// Reference to sensor panel.
        /// </summary>
        [SerializeField] private Panel SensorPanel;
        
        /// <summary>
        /// Reference to weather panel.
        /// </summary>
        [SerializeField] private Panel WeatherPanel;

        private void Start()
        {
            transform.DetachChildren();
            
            InitPanel(ButtonPanel, "light");
            InitPanel(CalendarPanel, "calendar");
            InitPanel(SensorPanel, "sensor");
            InitPanel(WeatherPanel, "weather");
        }

        /// <summary>
        /// Initializes a panel with a unique ID and type.
        /// </summary>
        /// <param name="panel">Panel to initialize</param>
        /// <param name="type">Panel type</param>
        private void InitPanel(Panel panel, string type)
        {
            // Generate a unique ID for the new panel
            string id = ((DateTimeOffset)DateTime.Now).ToUnixTimeMilliseconds().ToString();
            // Create new PanelData with type.demo format
            PanelData panelData = new("demo" + type + id, type + ".demo", panel.transform.position, true);
            // Add the new panel data to the manager
            PanelManager.Instance.AddNewPanelData(panelData);

            // Initialize the panel with the new data
            panel.InitPanel(panelData);
        }
    }
}