using System;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class GameManager : Singleton<GameManager>
{
    public string HassURL { get; private set; }

    public string Token { get; private set; }

    private void Start()
    {
        ZPlayerPrefs.Initialize("Hass", "Password");
        HassURL = SecurePlayerPrefs.GetString("HassURL");
        Token = SecurePlayerPrefs.GetString("Token");
    }
}
