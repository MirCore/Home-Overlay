using System;
using Managers;
using Proyecto26;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

public class GameManager : Singleton<GameManager>
{
    public Uri HassUri { get; private set; }
    public string HassURL { get; private set; }
    
    public string HassPort { get; private set; }

    public string HassToken { get; private set; }

    public HassEntity[] HassStates;

    private void OnEnable()
    {
        ZPlayerPrefs.Initialize("Hass", "Password");
        LoadConnectionSettings();
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
        LoadConnectionSettings();
        Debug.Log("Saved Settings");
    }
    
    private void LoadConnectionSettings()
    {
        HassURL = SecurePlayerPrefs.GetString("HassURL");
        HassPort = SecurePlayerPrefs.GetString("HassPort");
        HassToken = SecurePlayerPrefs.GetString("HassToken");
        HassUri =  new Uri($"{HassURL.TrimEnd('/')}:{HassPort}/api/");
    }

    public void OnHassStatesResponse(string responseText)
    {
        Debug.Log(responseText);
        HassStates = JsonHelper.ArrayFromJson<HassEntity>(responseText);
        foreach (HassEntity entity in HassStates)
        {
            string type = entity.entity_id.Split('.')[0].ToUpper();
            
            if (Enum.TryParse(type, out EDeviceType deviceType))
            {
                entity.DeviceType = deviceType;
            }
        }
        EventManager.InvokeOnHassStatesChanged();
    }
}
