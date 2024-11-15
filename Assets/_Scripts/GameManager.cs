using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class GameManager : Singleton<GameManager>
{
    public string HassURL { get; private set; }
    
    public string HassPort { get; private set; }

    public string HassToken { get; private set; }

    private void OnEnable()
    {
        ZPlayerPrefs.Initialize("Hass", "Password");
        HassURL = SecurePlayerPrefs.GetString("HassURL");
        HassPort = SecurePlayerPrefs.GetString("HassPort");
        HassToken = SecurePlayerPrefs.GetString("HassToken");
    }

    public void TestConnection(string url, int port, string token)
    {
        RestHandler.TestConnection(url, port, token);
    }

    public void SaveConnectionSettings(string url, int port, string token)
    {
        if (url != "")
            SecurePlayerPrefs.SetString("HassURL", url);
        if (port != 0)
            SecurePlayerPrefs.SetInt("HassPort", port);
        if (token != "")
            SecurePlayerPrefs.SetString("HassToken", token);
        Debug.Log("Saved Settings");
    }
}
