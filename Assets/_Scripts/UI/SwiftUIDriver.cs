using System;
using System.Collections.Generic;
using System.Linq;
using AOT;
using Managers;
using Proyecto26;
using Unity.PolySpatial;
using UnityEngine;
using UnityEngine.InputSystem;
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

        private static void OnConnectionTested(int status)
        {
            string message = status is 200 or 201 ? "Connection successful." : $"Connection error: {status}; {HttpStatusCodes.GetDescription(status)}";

            SetSwiftUIConnectionStatus(status, message);
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
                Debug.Log($"Received Command: {command}\n" +
                          $"Received arg0: {arg0}\n" +
                          $"Received arg1: {arg1}\n" +
                          $"Received arg2: {arg2}");
                
                // This could be stored in a static field or a singleton.
                // If you need to deal with multiple windows and need to distinguish between them,
                // you could add an ID to this callback and use that to distinguish windows.
                if (!_settingsUI)
                    _settingsUI = FindFirstObjectByType<SettingsUI>();

                int.TryParse(arg1, out int portInt);

                switch (command)
                {
                    case "getEntities":
                        SendEntitiesToSwiftUI();
                        return;
                    case "getConnectionValues":
                        SendConnectionValuesToSwiftUI();
                        return;
                    case "testConnection":
                        _settingsUI.TestConnection(arg0, portInt, arg2);
                        return;
                    case "saveConnection":
                        _settingsUI.SaveConnectionSettings(arg0, portInt, arg2);
                        return;
                }
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private static void SendConnectionValuesToSwiftUI()
        {
            SetSwiftUIConnectionValues(GameManager.Instance.HassURL, GameManager.Instance.HassPort.ToString(), GameManager.Instance.HassToken);
        }

        private static void SendEntitiesToSwiftUI()
        {
            Dictionary<string, HassState> hassStates = HassStates.GetHassStates();

            Dictionary<string, string> dict = hassStates.ToDictionary(state => state.Key,
                state => string.IsNullOrEmpty(state.Value.attributes.friendly_name)
                    ? state.Key.Split(".")[1]
                    : state.Value.attributes.friendly_name)
                .OrderBy(pair => pair.Value, StringComparer.OrdinalIgnoreCase).ToDictionary(pair => pair.Key, pair => pair.Value);
            
            string json = JsonHelpers.DictToJsonString(dict);

            Debug.Log("sending: " + json);
            
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
                    UIManager.Instance.ShowHomeButton();
                    CloseSwiftUIWindow("MainMenu"); //Imported native function located in the SwiftUIPlugin.swift
                    break;
                case VolumeCamera.WindowEvent.Focused:
                    Debug.Log("Window (un)focused");
                    GameManager.OnWindowFocused(windowState.IsFocused);
                    if (windowState.IsFocused == false)
                    {
                        Debug.Log("Window unfocused");
                        UIManager.Instance.ShowHomeButton();
                        CloseSwiftUIWindow("MainMenu");
                    }
                    break;
                case VolumeCamera.WindowEvent.Resized:
                case VolumeCamera.WindowEvent.Closed:
                default:
                    break;
            }
        }
            
#if UNITY_VISIONOS 
        [DllImport("__Internal")]
        static extern void SetNativeCallback(SwiftCallbackDelegate swiftCallback);

        [DllImport("__Internal")]
        public static extern void OpenSwiftUIWindow(string name);

        [DllImport("__Internal")]
        public static extern void CloseSwiftUIWindow(string name);

        [DllImport("__Internal")]
        private static extern void SetSwiftUIHassEntities(string name);
        
        [DllImport("__Internal")]
        private static extern void SetSwiftUIConnectionValues(string url, string port, string token);
        
        [DllImport("__Internal")]
        private static extern void SetSwiftUIConnectionStatus(int status, string message);
#else
        private static void SetNativeCallback(SwiftCallbackDelegate swiftCallback)
        {
        }

        internal static void OpenSwiftUIWindow(string name)
        {
        }

        internal static void CloseSwiftUIWindow(string name)
        {
        }
#endif
    }
}