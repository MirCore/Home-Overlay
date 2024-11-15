using System;
using System.Collections.Generic;
using System.IO;
using Managers;
using Models;
using Proyecto26;
using UnityEditor;
using UnityEngine;

public abstract class RestHandler
{
    
    
    private void Start()
    {
        string url = GameManager.Instance.HassURL;
        string port = GameManager.Instance.HassPort;
        string token = GameManager.Instance.HassToken;

        Uri uri = new ($"{url}:{port}/api/");
        
        
        RequestHelper post = new()
        {
            Uri = new Uri(uri, "services/input_boolean/toggle").ToString(),
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            },
            Body = new EntityID{ entity_id = "input_boolean.toggle"}
        };
        RestClient.Post(post).Then(response => {
                Debug.Log("Response: " + response.Text);
        })
        .Catch(err => {
            Debug.LogError("Error: " + err.Message);
        });
    }

    public static void TestConnection(string url, int port, string token)
    {
        Uri uri = new ($"{url.TrimEnd('/')}:{port}/api/");
        
        RequestHelper get = new()
        {
            Uri = uri.ToString(),
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            }
        };
        
        RestClient.Get(get).Then(response => {
            EventManager.InvokeOnConnectionTested(response.StatusCode.ToString());
        })
        .Catch(err => {
            EventManager.InvokeOnConnectionTested(err.Message);
        });
    }
}

[System.Serializable]
public class EntityID
{
    // ReSharper disable once InconsistentNaming
    public string entity_id;
}