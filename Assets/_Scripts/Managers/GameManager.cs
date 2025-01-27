using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT;
#if QUEST_BUILD && FALSE
using Meta.XR.MRUtilityKit;
#endif
using Structs;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Utils;


namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public Uri HassUri { get; private set; }
        public string HassURL { get; private set; }
        public int HassPort { get; private set; }
        public string HassToken { get; private set; }
        [field: SerializeField] public ARPlaneManager ARPlaneManager { get; private set; }
        [field: SerializeField] public ARAnchorManager ARAnchorManager { get; private set; }
        [field: SerializeField] public ARRaycastManager ARRaycastManager { get; private set; }

        /// <summary>
        /// Only used for debugging in the inspector.
        /// </summary>
        [SerializeField] public HassEntity[] InspectorHassStates;
        
        [SerializeField] internal List<EntityObject> EntityObjects;
        
        /// <summary>
        /// The EntitySpawner that spawns new entities.
        /// </summary>
        [SerializeField] private EntitySpawner EntitySpawner;

        [SerializeField] private GameObject HassUI;
        
#if QUEST_BUILD && FALSE
        [field: SerializeField] public EffectMesh EffectMesh { get; private set; }
#endif

        private void OnEnable()
        {
            // Initialize PlayerPrefs
            ZPlayerPrefs.Initialize("Hass", "Password");
            
            // Load saved connection settings
            LoadConnectionSettings();
            
            ARAnchorManager.trackablesChanged.AddListener(OnAnchorChanged);
        }

        private void OnDisable()
        {
            ARAnchorManager.trackablesChanged.RemoveListener(OnAnchorChanged);
        }

        /// <summary>
        /// Listens for when AR anchors are added to the scene, and deletes any anchors that aren't associated with an EntityObject.
        /// </summary>
        private void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
        {
            foreach (ARAnchor addedAnchor in changes.added.Where(addedAnchor =>
                         !EntityObjects.Exists(e => e.AnchorID == addedAnchor.trackableId.ToString())))
            {
                StartCoroutine(DeleteAnchorNextFrame(addedAnchor));
            }
        }

        /// <summary>
        /// Deletes an anchor on the next frame after it is added, after the anchor has been fully initialized.
        /// This is necessary because the anchor isn't fully initialized until the next frame after it is added,
        /// and attempting to delete it immediately will fail.
        /// </summary>
        /// <param name="anchor">The anchor to delete.</param>
        private IEnumerator DeleteAnchorNextFrame(ARAnchor anchor)
        {
            yield return new WaitForEndOfFrame();
            ARAnchorManager.TryRemoveAnchor(anchor);
        }

        private void OnInitialConnectionTested(int statusCode)
        {
            EventManager.OnConnectionTested -= OnInitialConnectionTested;

            if (statusCode is 200 or 201)
            {
                RestHandler.SetDefaultHeaders();
                RestHandler.GetHassEntities();
            }
            LoadEntityObjects();
        }

        private void LoadEntityObjects()
        {
            EntityObjects = SaveFile.ReadFile();
            if (EntityObjects == null)
                return;
            foreach (EntityObject entityObject in EntityObjects)
            {
                EntitySpawner.SpawnSavedEntity(entityObject);
            }

            EventManager.InvokeOnAppStateLoaded();
        }

        /// <summary>
        /// Tests the connection to Home Assistant using the provided URL, port, and token.
        /// </summary>
        /// <param name="url">The base URL of Home Assistant.</param>
        /// <param name="port">The port number of Home Assistant.</param>
        /// <param name="token">The authorization token of Home Assistant.</param>
        private static void TestConnection(string url, int port, string token)
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
            HassPort = SecurePlayerPrefs.GetInt("HassPort");
            HassToken = SecurePlayerPrefs.GetString("HassToken");

            if (HassURL != "")
            {
                try
                {
                    HassUri = new Uri($"{HassURL.TrimEnd('/')}:{HassPort}/api/");
                }
                catch (UriFormatException)
                {
                    Debug.LogError("The URL is not valid.");
                }
            }
            
            // If no connection settings are saved, show the settings/connect tab
            // Otherwise, test the connection
            if (HassURL == "" || HassPort == 0 || HassToken == "")
            {
                UIManager.Instance.ShowSettingsTab();
                EventManager.InvokeOnConnectionTested(412); // 412 Precondition Failed
            }
            else
            {
                TestConnection(HassURL, HassPort, HassToken);
                EventManager.OnConnectionTested += OnInitialConnectionTested;
            }
        }

        public void RemoveEntity(EntityObject entityObject)
        {
            EntityObjects.Remove(entityObject);
            SaveFile.SaveEntityObjects();
        }

        public void AddEntity(EntityObject eo, Entity entity)
        {
            EntityObject entityObject = EntityObjects.FirstOrDefault(e => e == eo);

            if (entityObject != null)
                entityObject.Entity = entity;
        }
    }
}
