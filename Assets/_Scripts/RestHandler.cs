using System;
using System.Collections.Generic;
using System.IO;
using Managers;
using Models;
using Proyecto26;
using UI;
using UnityEditor;
using UnityEngine;
using Uri = System.Uri;

public abstract class RestHandler
{
    
    
    private void Start()
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/input_boolean/toggle");
        string token = GameManager.Instance.HassToken;
        
        RequestHelper post = new()
        {
            Uri = uri.ToString(),
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

    public static void GetHassEntities()
    {
        Uri uri = new (GameManager.Instance.HassUri, "states");
        string token = GameManager.Instance.HassToken;
        
        RequestHelper get = new()
        {
            Uri = uri.ToString(),
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            }
        };
        
        RestClient.Get(get)
            .Then(response => {
                GameManager.Instance.OnHassStatesResponse(response.Text);
            })
            .Catch(err => {
                Debug.LogError("Error: " + err.Message);
            });
    }
}

[Serializable]
public class EntityID
{
    // ReSharper disable once InconsistentNaming
    public string entity_id;
}