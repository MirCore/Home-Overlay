using System;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Managers;
using Proyecto26;
using Unity.PolySpatial;
using UnityEngine;
using Utils;

#if UNITY_VISIONOS 
using System.Runtime.InteropServices;
#endif

namespace UI
{
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
                        PanelManager.Instance.SpawnNewEntity(arg0);
                        CloseSwiftMainUI();
                        return;
                    case "getPanels":
                        SendPanelsToSwiftUI();
                        return;
                    case "deletePanel":
                        PanelManager.Instance.DeletePanel(arg0);
                        SendPanelsToSwiftUI();
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

        private static void OnConnectionTested(int status, Uri uri)
        {
            string message = status is 200 or 201 ? "Connection successful." : $"Connection error: {status}; {HttpStatusCodes.GetDescription(status)}";
            
            SetSwiftUIConnectionStatus(status, message, uri.ToString());
        }

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

            Debug.Log("sending: " + json);

            SetSwiftUIPanels(json);
        }


        private static void CloseSwiftMainUI()
        {
            CloseSwiftUIWindow("MainMenu");
            UIManager.Instance.ShowHomeButton();
        }

        private static void SendConnectionValuesToSwiftUI()
        {
            SetSwiftUIConnectionValues(GameManager.Instance.HassURL, GameManager.Instance.HassPort.ToString(), GameManager.Instance.HassToken);
        }

        private static void SendEntitiesToSwiftUI()
        {
            Dictionary<string, HassState> hassStates = HassStates.GetHassStates();

            
            List<JsonData> jsonData = hassStates.Select(entity => new JsonData
            {
                entityId = entity.Key, 
                name = entity.Value.attributes.friendly_name
            }).ToList();
            
            string json = JsonHelper.ArrayToJsonString(jsonData.ToArray());
            
            SetSwiftUIHassEntities(json);
        }

        public void WindowEvent(VolumeCamera volumeCamera, VolumeCamera.WindowState windowState) //Set this public method into your VolumeCamera>Events in the inspector, dont forget to Choose Dynamic WindowState!
        {
            switch (windowState.WindowEvent)
            {
                case VolumeCamera.WindowEvent.Opened: //Open window when your app is on focus
                    Debug.Log("Window opened");
                    break;
                case VolumeCamera.WindowEvent.Backgrounded: //Close window when your app is on background
                    Debug.Log("Window Backgrounded");
                    CloseSwiftMainUI();
                    break;
                case VolumeCamera.WindowEvent.Focused:
                    Debug.Log("Window (un)focused");
                    GameManager.OnWindowFocused(windowState.IsFocused);
                    if (windowState.IsFocused == false)
                    {
                        Debug.Log("Window unfocused");
                        CloseSwiftMainUI();
                    }
                    break;
                case VolumeCamera.WindowEvent.Closed:
                case VolumeCamera.WindowEvent.Resized:
                default:
                    break;
            }
        }
            
#if UNITY_VISIONOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        static extern void SetNativeCallback(SwiftCallbackDelegate swiftCallback);
        
        [DllImport("__Internal")]
        public static extern void OpenSwiftUIWindow(string name, string tab = "");

        [DllImport("__Internal")]
        public static extern void CloseSwiftUIWindow(string name);

        [DllImport("__Internal")]
        private static extern void SetSwiftUIHassEntities(string json);

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
        
        private static void SetSwiftUIHassEntities(string json){}

        private static void SetSwiftUIPanels(string json){}
        
        private static void SetSwiftUIConnectionValues(string url, string port, string token){}
        
        private static void SetSwiftUIConnectionStatus(int status, string message, string savedUri){}
#endif
        
        
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