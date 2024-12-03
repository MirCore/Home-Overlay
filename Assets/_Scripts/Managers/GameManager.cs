using System;
using System.Collections.Generic;
using Meta.XR.MRUtilityKit;
using Structs;
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

        [SerializeField] public HassEntity[] InspectorHassStates;
        [SerializeField] internal List<EntityObject> EntityObjects;
        
        /// <summary>
        /// The EntitySpawner that spawns new entities.
        /// </summary>
        [SerializeField] private EntitySpawner EntitySpawner;
        [field: SerializeField] public EffectMesh EffectMesh { get; private set; }

        private void OnEnable()
        {
            ZPlayerPrefs.Initialize("Hass", "Password");
            LoadConnectionSettings();
            LoadEntityObjects();

            RestHandler.SetDefaultHeaders();
        }

        private void LoadEntityObjects()
        {
            EntityObjects = SaveFile.ReadFile();
            if (EntityObjects == null)
                return;
            foreach (EntityObject entityObject in EntityObjects)
            {
                EntitySpawner.SpawnSavedEntity(entityObject.Position, entityObject);
            }
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
            if (HassURL != "")
                HassUri = new Uri($"{HassURL.TrimEnd('/')}:{HassPort}/api/");
        }

        public void RemoveEntity(EntityObject entityObject)
        {
            EntityObjects.Remove(entityObject);
            SaveFile.SaveEntityObjects();
        }
    }
}
