using System;
using UnityEngine;

namespace Utils
{
    public static class SecurePlayerPrefs
    {
        // Sets a string value in PlayerPrefs
        public static void SetString(string key, string value)
        {
            // Store the value
            ZPlayerPrefs.SetString(key, value);
        }
    
        // Sets an int value in PlayerPrefs
        public static void SetInt(string key, int value)
        {
            // Store the value
            ZPlayerPrefs.SetInt(key, value);
        }

        // Retrieves a string value from PlayerPrefs
        public static string GetString(string key, string defaultValue = "")
        {
            // Get the value
            string value = ZPlayerPrefs.GetString(key, defaultValue);

            return value;
        }

        // Retrieves a string value from PlayerPrefs
        public static int GetInt(string key, int defaultValue = 0)
        {
            int value = 0;
            // Get the value
            try
            {
                value = ZPlayerPrefs.GetInt(key, defaultValue);
            }
            catch (Exception e)
            {
                Debug.LogWarning("Failed to decrypt PlayerPrefs key: " + e.Message);
            }

            return value;
        }
    }
}