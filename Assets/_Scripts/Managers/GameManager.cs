using System;
using System.Collections.Generic;
using System.Linq;
using Proyecto26;
using UnityEngine;
using Utils;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public Uri HassUri { get; private set; }
        public string HassURL { get; private set; }
        public string HassPort { get; private set; }
        public string HassToken { get; private set; }

        private readonly Dictionary<string, HassEntity> _hassStates = new();

        [SerializeField] private HassEntity[] InspectorHassStates;

        private void OnEnable()
        {
            ZPlayerPrefs.Initialize("Hass", "Password");
            LoadConnectionSettings();

            RestHandler.SetDefaultHeaders();
        }

        /// <summary>
        /// Tests the connection to Home Assistant using the provided URL, port, and token.
        /// </summary>
        /// <param name="url">The base URL of Home Assistant.</param>
        /// <param name="port">The port number of Home Assistant.</param>
        /// <param name="token">The authorization token of Home Assistant.</param>
        public static void TestConnection(string url, int port, string token)
        {
            RestHandler.TestConnection(url, port, token);
        }

        /// <summary>
        /// Saves the connection settings for Home Assistant.
        /// </summary>
        /// <param name="url">The base URL of Home Assistant.</param>
        /// <param name="port">The port number of Home Assistant.</param>
        /// <param name="token">The authorization token of Home Assistant.</param>
        public void SaveConnectionSettings(string url, int port, string token)
        {
            if (url != "")
                SecurePlayerPrefs.SetString("HassURL", url);
            if (port != 0)
                SecurePlayerPrefs.SetInt("HassPort", port);
            if (token != "")
                SecurePlayerPrefs.SetString("HassToken", token);
            LoadConnectionSettings();
            Debug.Log("Saved Settings");
        }

        /// <summary>
        /// Loads the connection settings from secure storage.
        /// </summary>
        private void LoadConnectionSettings()
        {
            HassURL = SecurePlayerPrefs.GetString("HassURL");
            HassPort = SecurePlayerPrefs.GetString("HassPort");
            HassToken = SecurePlayerPrefs.GetString("HassToken");
            HassUri = new Uri($"{HassURL.TrimEnd('/')}:{HassPort}/api/");
        }

        /// <summary>
        /// Handles the response from Home Assistant and updates the state entities.
        /// </summary>
        /// <param name="responseText">The response text from Home Assistant containing entity states.</param>
        public void OnHassStatesResponse(string responseText)
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
                _hassStates[entity.entity_id] = entity;
            }
            
            // Invoke the event that the Hass states have changed
            EventManager.InvokeOnHassStatesChanged();
            
            // Copy the entities to the inspector field for debugging
            InspectorHassStates = _hassStates.Values.ToArray();
        }

        public HassEntity GetHassState(string entityID)
        {
            if (string.IsNullOrEmpty(entityID))
                return null;
            HassEntity state = _hassStates[entityID];
            return state ?? new HassEntity();
        }

        public IEnumerable<KeyValuePair<string, HassEntity>> GetHassStates()
        {
            return _hassStates;
        }
    }
}