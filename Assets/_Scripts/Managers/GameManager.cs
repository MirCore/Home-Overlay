using System;
using System.Threading;
using System.Threading.Tasks;
using Structs;
using UnityEngine;
using Utils;

namespace Managers
{
    /// <summary>
    /// Manages the app state and connection settings for Home Assistant integration.
    /// </summary>
    public class GameManager : Singleton<GameManager>
    {
        /// <summary>
        /// The URI for the Home Assistant API.
        /// </summary>
        public Uri HassUri { get; private set; }

        /// <summary>
        /// The base URL of the Home Assistant instance.
        /// </summary>
        public string HassURL { get; private set; } = "";

        /// <summary>
        /// The port number of the Home Assistant instance.
        /// </summary>
        public int HassPort { get; private set; }

        /// <summary>
        /// The authorization token for the Home Assistant API.
        /// </summary>
        public string HassToken { get; private set; } = "";

        /// <summary>
        /// The refresh rate of the Home Assistant API in seconds.
        /// </summary>
        [Tooltip("The refresh rate of the Home Assistant API in seconds.")]
        [field: SerializeField] public int HassStateRefreshRate { get; private set; } = 10;
        
        private CancellationTokenSource _cancellationTokenSource;
        

        
#if UNITY_EDITOR
        [Header("Debugging")]
        public bool DebugLogPostResponses; // Flag to enable logging of POST responses for debugging.
        public bool DebugLogGetHassEntities; // Flag to enable logging of GET Hass entities for debugging.
        [SerializeField] public HassState[] InspectorHassStates; // Only used for debugging in the inspector.
#endif

        private void OnEnable()
        {
            // Initialize PlayerPrefs
            ZPlayerPrefs.Initialize("Hass", "Password");
        }

        private void Start()
        {
            // Load saved connection settings
            LoadConnectionSettings();
        }

        /// <summary>
        /// Cancels any ongoing tasks when the application quits.
        /// </summary>
        private void OnApplicationQuit()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Cancels any ongoing tasks when the GameManager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
        }

        /// <summary>
        /// Periodically retrieves the Home Assistant states.
        /// </summary>
        /// <param name="cancellationToken">The token for cancellation requests.</param>
        private async Task GetHassStatesPeriodicallyAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {                
                // Wait for the specified time before retrieving states
                await Task.Delay(TimeSpan.FromSeconds(HassStateRefreshRate), cancellationToken);
                RestHandler.GetHassEntities();
            }
        }
        
        /// <summary>
        /// Handles the initial connection test event.
        /// </summary>
        /// <param name="statusCode">The HTTP status code of the connection test.</param>
        /// <param name="uri">The URI used for the connection test.</param>
        private void OnInitialConnectionTested(int statusCode, Uri uri)
        {
            EventManager.OnConnectionTested -= OnInitialConnectionTested;
            
            // Check if the connection was successful
            if (statusCode is 200 or 201)
            {                
                // Set default headers, retrieve Home Assistant config and retrieve Home Assistant entities
                RestHandler.SetDefaultHeaders();
                RestHandler.GetHassConfig();
                RestHandler.GetHassEntities();
            }            
            
            // (Re)start periodic retrieval of Home Assistant states
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _ = GetHassStatesPeriodicallyAsync(_cancellationTokenSource.Token);
            
            // Invoke the event to indicate that the app state has loaded
            EventManager.InvokeOnAppStateLoaded();
        }

        /// <summary>
        /// Saves the connection settings for Home Assistant.
        /// </summary>
        /// <param name="url">The base URL of Home Assistant.</param>
        /// <param name="port">The port number of Home Assistant.</param>
        /// <param name="token">The authorization token of Home Assistant.</param>
        public void SaveConnectionSettings(string url, int port, string token)
        {
            // Save the connection settings
            if (url != "")
                SecurePlayerPrefs.SetString("HassURL", url);
            if (port != 0)
                SecurePlayerPrefs.SetInt("HassPort", port);
            if (token != "")
                SecurePlayerPrefs.SetString("HassToken", token);
            
            // Reload the connection settings
            LoadConnectionSettings();
        }

        /// <summary>
        /// Loads the connection settings.
        /// </summary>
        private void LoadConnectionSettings()
        {
            // Retrieve the connection settings
            HassURL = SecurePlayerPrefs.GetString("HassURL");
            HassPort = SecurePlayerPrefs.GetInt("HassPort");
            HassToken = SecurePlayerPrefs.GetString("HassToken");
            
            // Construct the URI for the Home Assistant API
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
                EventManager.OnConnectionTested += OnInitialConnectionTested;
                EventManager.InvokeOnConnectionTested(412); // 412 Precondition Failed
            }
            else
            {
                RestHandler.TestConnection(HassURL, HassPort, HassToken);
                EventManager.OnConnectionTested += OnInitialConnectionTested;
            }
        }

        /// <summary>
        /// Handles the event when the application window gains or loses focus.
        /// This ensures that the HomeButtonUI shows up when the app lost focus on AVP
        /// </summary>
        /// <param name="isFocused">True if the window is focused, false otherwise.</param>
        public static void OnWindowFocused(bool isFocused)
        {
            if (!isFocused)
                UIManager.Instance.CloseMainMenu();
        }
    }
}
