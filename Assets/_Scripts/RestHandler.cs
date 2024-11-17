using System;
using System.Collections.Generic;
using Managers;
using Proyecto26;
using UnityEngine;
using Uri = System.Uri;

/// <summary>
/// Handles sending REST requests to Home Assistant.
/// </summary>
public abstract class RestHandler
{
    /// <summary>
    /// Tests the connection to Home Assistant.
    /// 
    /// The connection test is done by sending a GET request to the Home Assistant API.
    /// The response is then passed to the <see cref="EventManager.OnConnectionTested"/> method.
    /// </summary>
    /// <param name="url">The base URL of Home Assistant.</param>
    /// <param name="port">The port number of Home Assistant.</param>
    /// <param name="token">The authorization token of Home Assistant.</param>
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

    /// <summary>
    /// Gets the Home Assistant entities.
    /// </summary>
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
                Debug.LogError("Error: " + err.Message + err.StackTrace);
            });
    }

    /// <summary>
    /// Toggles the light with the given entity ID.
    /// </summary>
    /// <param name="entityID">The entity ID of the light to toggle.</param>
    public static void ToggleLight(string entityID)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/light/toggle");
        string token = GameManager.Instance.HassToken;
        
        RequestHelper post = new()
        {
            Uri = uri.ToString(),
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            },
            Body = new EntityID{ entity_id = entityID}
        };
        
        RestClient.Post(post).Then(response => {
                GameManager.Instance.OnHassStatesResponse(response.Text);
            })
            .Catch(err => {
                Debug.LogError("Error: " + err.Message);
            });
    }
}

/// <summary>
/// Represents an entity ID.
/// </summary>
[Serializable]
public class EntityID
{
    // ReSharper disable once InconsistentNaming
    /// <summary>
    /// The entity ID.
    /// </summary>
    public string entity_id;
}