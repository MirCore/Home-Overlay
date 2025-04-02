using System;
using System.Collections;
using UnityEngine;
using Utils;


namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        public Uri HassUri { get; private set; }
        public string HassURL { get; private set; } = "";
        public int HassPort { get; private set; }
        public string HassToken { get; private set; } = "";

        [Tooltip("The refresh rate of the Home Assistant API in seconds.")]
        [SerializeField] private int HassStateRefreshRate = 10;
        
        private DateTime _lastHassStateRefresh;

        
        [field: SerializeField] public HassConfig HassConfig { get; private set; }
        
        /// <summary>
        /// The PanelSpawner that spawns new entities.
        /// </summary>
        [SerializeField] private PanelSpawner PanelSpawner;

        [SerializeField] private GameObject HassUI;
        
        [Header("Debugging")]
        public bool DebugLogPostResponses;
        public bool DebugLogGetHassEntities;
        /// <summary>
        /// Only used for debugging in the inspector.
        /// </summary>
        [SerializeField] public HassState[] InspectorHassStates;

        private void OnEnable()
        {
            // Initialize PlayerPrefs
            ZPlayerPrefs.Initialize("Hass", "Password");
        }

        private void Start()
        {
            // Load saved connection settings
            LoadConnectionSettings();
            EventManager.OnConnectionTested += OnConnectionTested;
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        private void OnDisable()
        {
            EventManager.OnConnectionTested -= OnConnectionTested;
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        private void OnHassStatesChanged()
        {
            _lastHassStateRefresh = DateTime.Now;
        }

        private void OnConnectionTested(int response)
        {
            if (response is 200 or 201)
            {
                RestHandler.GetHassConfig();
                StartCoroutine(GetHassStatesPeriodically());
            }
        }
        
        // ReSharper disable Unity.PerformanceAnalysis
        private IEnumerator GetHassStatesPeriodically()
        {
            while (true)
            {
                yield return new WaitForSeconds(HassStateRefreshRate);
                RestHandler.GetHassEntities();
            }
        }

        private void OnInitialConnectionTested(int statusCode)
        {
            EventManager.OnConnectionTested -= OnInitialConnectionTested;

            if (statusCode is 200 or 201)
            {
                RestHandler.SetDefaultHeaders();
                RestHandler.GetHassEntities();
            }
            PanelManager.Instance.LoadEntityObjects();
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

        public bool HassStatesRecentlyUpdated()
        {
            return _lastHassStateRefresh.AddSeconds((float)HassStateRefreshRate / 2) > DateTime.Now;
        }

        public void OnHassConfigLoaded(HassConfig config)
        {
            HassConfig = config;
        }

        public static void OnWindowFocused(bool isFocused)
        {
            if (!isFocused)
                UIManager.Instance.CloseMainMenu();
        }
    }
}
