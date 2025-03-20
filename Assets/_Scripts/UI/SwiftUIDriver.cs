using System;
using AOT;
using UnityEngine;
using Utils;

#if UNITY_VISIONOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace UI
{
    public class SwiftUIDriver : Singleton<SwiftUIDriver>

    {
    private void OnEnable()
    {
        SetNativeCallback(SettingsCallbackFromNative);
    }

    private delegate void SwiftCallbackDelegate(string command, string url, string port, string token);

    // This attribute is required for methods that are going to be called from native code
    // via a function pointer.

    [MonoPInvokeCallback(typeof(SwiftCallbackDelegate))]
    private static void SettingsCallbackFromNative(string command, string url, string port, string token)
    {
        // MonoPInvokeCallback methods will leak exceptions and cause crashes; always use a try/catch in these methods
        try
        {
            Debug.Log($"Received Command: {command}");
            Debug.Log($"Received URL: {url}");
            Debug.Log($"Received Port: {port}");
            Debug.Log($"Received Token: {token}");

            // This could be stored in a static field or a singleton.
            // If you need to deal with multiple windows and need to distinguish between them,
            // you could add an ID to this callback and use that to distinguish windows.
            SettingsUI self = FindFirstObjectByType<SettingsUI>();

            int.TryParse(port, out int portInt);

            switch (command)
            {
                case "test connection":
                    self.TestConnection(url, portInt, token);
                    return;
                case "save connection":
                    self.SaveConnectionSettings(url, portInt, token);
                    return;
            }
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
        }
    }

#if UNITY_VISIONOS && !UNITY_EDITOR
        public void WindowEvent(WindowState windowState) //Set this public method into your VolumeCamera>Events in the inspector, dont forget to Choose Dynamic WindowState!
        {
            switch (windowState.WindowEvent)
            {
                case VolumeCamera.WindowEvent.Opened: //Open window when your app is on focus
                    GoForeground();
                    break;
                case VolumeCamera.WindowEvent.Backgrounded: //Close window when your app is on background
                    GoBackground();
                    break;
                default:
                    break;
            }
        }
        void GoForeground()
        {

        }
        void GoBackground()
        {
            CloseSwiftUIWindow("SettingsWindow"); //Imported native function located in your version of SwittUIPlugin.swift
            nativeWindowOpen = false;
        }

        [DllImport("__Internal")]
        static extern void SetNativeCallback(SwiftCallbackDelegate swiftCallback);

        [DllImport("__Internal")]
        static extern void OpenSwiftUISettingsWindow(string name, string url, string port, string token);

        [DllImport("__Internal")]
        static extern void CloseSwiftUIWindow(string name);

#else
    private static void SetNativeCallback(SwiftCallbackDelegate swiftCallback)
    {
    }

    internal static void OpenSwiftUISettingsWindow(string name, string url, string port, string token)
    {
    }

    internal static void CloseSwiftUIWindow(string name)
    {
    }

#endif
    }
}