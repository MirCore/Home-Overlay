using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Managers;
using Proyecto26;
using Structs;
using UnityEngine;
using Utils;

#if UNITY_VISIONOS 
using Unity.PolySpatial;
using System.Runtime.InteropServices;
#endif

namespace UI
{
    /// <summary>
    /// Handles communication between Unity and SwiftUI, managing UI windows and data exchange.
    /// </summary>
    public class SwiftUIDriver : Singleton<SwiftUIDriver>
    {
        private static SettingsUI _settingsUI;
        private void OnEnable()
        {
            SetNativeCallback(CallbackFromNative);
            EventManager.OnConnectionTested += OnConnectionTested;
        }

        private void OnDisable()
        {
            EventManager.OnConnectionTested -= OnConnectionTested;
        }

        private static void OnConnectionTested(int status, Uri uri)
        {
            string message = status is 200 or 201 ? "Connection successful." : $"Connection error: {status}; {HttpStatusCodes.GetDescription(status)}";
            string url = uri == null ? "" : uri.ToString();
            SetSwiftUIConnectionStatus(status, message, url);
        }
        
        private static void CloseSwiftMainUI()
        {
            CloseSwiftUIWindow("MainMenu");
            UIManager.Instance.ShowHomeButton();
            SoundManager.Instance.ResetAudio();
        }
        
        #region Swift Callbacks

        private delegate void SwiftCallbackDelegate(string command, string url, string port, string token);

        // This attribute is required for methods that are going to be called from native code
        // via a function pointer.

        [MonoPInvokeCallback(typeof(SwiftCallbackDelegate))]
        private static void CallbackFromNative(string command, string arg0, string arg1, string arg2)
        {
            // MonoPInvokeCallback methods will leak exceptions and cause crashes; always use a try/catch in these methods
            try
            {
                int.TryParse(arg1, out int portInt);

                switch (command)
                {
                    case "getEntities":
                        SendEntitiesToSwiftUI();
                        return;
                    case "getConnectionValues":
                        SendConnectionValuesToSwiftUI();
                        return;
                    case "getConnectionStatus":
                        RestHandler.TestConnection(GameManager.Instance.HassURL, GameManager.Instance.HassPort, GameManager.Instance.HassToken);
                        return;
                    case "testConnection":
                        RestHandler.TestConnection(arg0, portInt, arg2);
                        return;
                    case "saveConnection":
                        GameManager.Instance.SaveConnectionSettings(arg0, portInt, arg2);
                        return;
                    case "windowClosed":
                        CloseSwiftMainUI();
                        return;
                    case "createEntity":
                        PanelManager.Instance.SpawnNewPanel(arg0);
                        CloseSwiftMainUI();
                        return;
                    case "getPanels":
                        SendPanelsToSwiftUI();
                        return;
                    case "deletePanel":
                        PanelManager.Instance.DeletePanel(arg0);
                        SendPanelsToSwiftUI();
                        return;
                    case "highlightPanel":
                        PanelManager.Instance.HighlightPanel(arg0);
                        return;
                    case "createDemoPanels":
                        PanelManager.Instance.CreateDemoPanels();
                        CloseSwiftMainUI();
                        return;
                    default:
                        Debug.Log(
                            $"Unhandled Command: {command}\n" +
                            $"Received arg0: {arg0}\nReceived arg1: {arg1}\nReceived arg2: {arg2}");
                        return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
        
#if UNITY_VISIONOS 
        public void WindowEvent(VolumeCamera volumeCamera, VolumeCamera.WindowState windowState) //Set this public method into your VolumeCamera>Events in the inspector, dont forget to Choose Dynamic WindowState!
        {
            switch (windowState.WindowEvent)
            {
                case VolumeCamera.WindowEvent.Opened: //Open window when your app is on focus
                    break;
                case VolumeCamera.WindowEvent.Backgrounded: //Close window when your app is on background
                    CloseSwiftMainUI();
                    break;
                case VolumeCamera.WindowEvent.Focused:
                    GameManager.OnWindowFocused(windowState.IsFocused);
                    if (windowState.IsFocused == false)
                    {
                        CloseSwiftMainUI();
                    }
                    break;
                case VolumeCamera.WindowEvent.Closed:
                case VolumeCamera.WindowEvent.Resized:
                default:
                    break;
            }
        }
#endif
        
        #endregion

        #region Send to Swift functions

        private static void SendPanelsToSwiftUI()
        {
            List<JsonData> jsonData = PanelManager.Instance.PanelDataList.Select(panelData => new JsonData
            {
                entityId = panelData.EntityID,
                panelId = panelData.ID, 
                name = HassStates.GetHassState(panelData.EntityID)?.attributes.friendly_name,
                active = panelData.Panel != null
            }).ToList();
            string json = JsonHelper.ArrayToJsonString(jsonData.ToArray());

            SetSwiftUIPanels(json);
        }
        
        private static void SendConnectionValuesToSwiftUI()
        {
            SetSwiftUIConnectionValues(GameManager.Instance.HassURL, GameManager.Instance.HassPort.ToString(), GameManager.Instance.HassToken);
        }

        private static void SendEntitiesToSwiftUI()
        {
            // Generate a List of supported device types
            string[] deviceTypes = Enum.GetValues(typeof(EDeviceType)).Cast<EDeviceType>()
                .Select(e => e.GetDisplayName()).ToArray();
            string deviceJson = JsonHelper.ArrayToJsonString(deviceTypes);
            
            // Generate a List of hass entities
            Dictionary<string, HassState> hassStates = HassStates.GetHassStates();
            JsonData[] jsonData = hassStates.Where(state => state.Value.DeviceType != EDeviceType.DEFAULT).Select(entity => new JsonData
            {
                entityId = entity.Key, 
                name = entity.Value.attributes.friendly_name
            }).ToArray();
            string entityJson = JsonHelper.ArrayToJsonString(jsonData);
            
            SetSwiftUIHassEntities(entityJson, deviceJson);
        }

        #endregion

        #region DllImports
            
#if UNITY_VISIONOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void SetNativeCallback(SwiftCallbackDelegate swiftCallback);
        
        [DllImport("__Internal")]
        public static extern void OpenSwiftUIWindow(string name, string tab = "");

        [DllImport("__Internal")]
        public static extern void CloseSwiftUIWindow(string name);

        [DllImport("__Internal")]
        private static extern void SetSwiftUIHassEntities(string entityJson, string deviceJson);

        [DllImport("__Internal")]
        private static extern void SetSwiftUIPanels(string json);
        
        [DllImport("__Internal")]
        private static extern void SetSwiftUIConnectionValues(string url, string port, string token);
        
        [DllImport("__Internal")]
        private static extern void SetSwiftUIConnectionStatus(int status, string message, string savedUri);
#else
        private static void SetNativeCallback(SwiftCallbackDelegate swiftCallback){}

        internal static void OpenSwiftUIWindow(string name, string tab = ""){}

        internal static void CloseSwiftUIWindow(string name){}
        
        private static void SetSwiftUIHassEntities(string entityJson, string deviceJson){}
        
        private static void SetSwiftUIPanels(string json){}
        
        private static void SetSwiftUIConnectionValues(string url, string port, string token){}
        
        private static void SetSwiftUIConnectionStatus(int status, string message, string savedUri){}
#endif
        
        #endregion
        
        [Serializable]
        public struct JsonData
        {
            public string entityId;
            public string panelId;
            public string name;
            public bool active;
        }
    }
}
