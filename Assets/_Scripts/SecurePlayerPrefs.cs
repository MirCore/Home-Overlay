public static class SecurePlayerPrefs
{
    // Sets a string value in PlayerPrefs
    public static void SetString(string key, string value)
    {
        // Store the value
        //PlayerPrefs.SetString(key, value); 
        ZPlayerPrefs.SetString(key, value);
    }

    // Retrieves a string value from PlayerPrefs
    public static string GetString(string key, string defaultValue = "")
    {
        // Get the value
        //string value = PlayerPrefs.GetString(key, defaultValue);
        string value = ZPlayerPrefs.GetString(key, defaultValue);

        return value;
    }
}