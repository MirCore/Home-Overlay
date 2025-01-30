using System;
using System.Collections.Generic;
using System.Net;
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
    /// Sets the default headers for REST requests to Home Assistant.
    /// 
    /// The default headers include the authorization token and the content type.
    /// </summary>
    public static void SetDefaultHeaders()
    {
        string token = GameManager.Instance.HassToken;
        
        RestClient.DefaultRequestHeaders = new Dictionary<string, string>
        {
            { "Authorization", $"Bearer {token}" },
            { "content-type", "application/json" }
        };
    }
    
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
        Uri uri;

        // Validate and construct the URI
        try
        {
            uri = new Uri($"{url.TrimEnd('/')}:{port}/api/");
        }
        catch (UriFormatException ex)
        {
            Debug.LogError($"Invalid URI: {ex.Message}");
            EventManager.InvokeOnConnectionTested(400); // Use 400 Bad Request for invalid URI
            return;
        }
        
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
            EventManager.InvokeOnConnectionTested((int)response.StatusCode);
        })
        .Catch(err => {
            if (err is RequestException requestException)
            {
                int statusCode = (int)requestException.StatusCode;
                EventManager.InvokeOnConnectionTested(statusCode);
            }
            else
            {
                EventManager.InvokeOnConnectionTested(err.HResult);
                Debug.LogError("Error: " + err.Message + "\n" + err.StackTrace);
            }
        });
    }

    #region Get


    /// <summary>
    /// Gets the Home Assistant entities.
    /// </summary>
    public static void GetHassEntities()
    {
        if (GameManager.Instance.HassStatesRecentlyUpdated())
            return;
        if (!RestClient.DefaultRequestHeaders.ContainsKey("Authorization"))
            return;
        if (GameManager.Instance.HassUri == null)
            return;
        
        RequestHelper get = new()
        {
            Uri = new Uri(GameManager.Instance.HassUri, "states").ToString(),
        };
        
        RestClient.Get(get)
            .Then(response => {
                HassStates.OnHassStatesResponse(response.Text);
            })
            .Catch(err => {
                Debug.LogError("Error: " + err.Message + "\n" + err.StackTrace);
            });
    }

    public static void GetHassConfig()
    {
        if (GameManager.Instance.HassStatesRecentlyUpdated())
            return;
        
        RequestHelper get = new()
        {
            Uri = new Uri(GameManager.Instance.HassUri, "config").ToString(),
        };
        
        RestClient.Get(get)
            .Then(response => {
                HassStates.OnHassConfigResponse(response.Text);
            })
            .Catch(err => {
                Debug.LogError("Error: " + err.Message + "\n" + err.StackTrace);
            });
    }
    
    #endregion

    #region Post
    
    /// <summary>
    /// Sends a POST request to the specified URI with the given entity ID.
    /// </summary>
    /// <param name="body">The data to include in the request body.</param>
    /// <param name="uri">The URI to send the POST request to.</param>
    private static void SendPostRequest(Uri uri, object body)
    {
        RequestHelper postRequest = new()
        {
            Uri = uri.ToString(),
            Body = body
        };
        
        RestClient.Post(postRequest)
            .Then(response => {
                HassStates.OnHassStatesResponse(response.Text);
                if (GameManager.Instance.DebugLogPostResponses)
                    Debug.Log(response.Text);
            })
            .Catch(err => {
                Debug.LogError("Error: " + err.Message + err.StackTrace);
            });
    }
    
    /// <summary>
    /// Toggles the light state for the specified entity ID.
    /// </summary>
    /// <param name="entityID">The entity ID of the light to toggle.</param>
    public static void ToggleLight(string entityID)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/light/toggle");
        EntityID body = new() { entity_id = entityID };
        SendPostRequest(uri, body);
    }

    /// <summary>
    /// Toggles the switch state for the specified entity ID.
    /// </summary>
    /// <param name="entityID">The entity ID of the switch to toggle.</param>
    public static void ToggleSwitch(string entityID)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/switch/toggle");
        EntityID body = new() { entity_id = entityID };
        SendPostRequest(uri, body);
    }

    public static void SetLightBrightness(string entityID, int value)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/light/turn_on");
        Brightness body = new() { entity_id = entityID, brightness = value.ToString() };
        SendPostRequest(uri, body);
    }


    public static void SetLightColor(string entityID, Color color)
    {
        Debug.Log(color);
        int[] rgb = { (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255) };
        Debug.Log("r: " + rgb[0] + " g: " + rgb[1] + " b: " + rgb[2]);
        Uri uri = new (GameManager.Instance.HassUri, "services/light/turn_on");
        RGBColor body = new() { entity_id = entityID, rgb_color = rgb };
        SendPostRequest(uri, body);
    }

    public static void SetLightTemperature(string entityID, int kelvin)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/light/turn_on");
        Kelvin body = new() { entity_id = entityID, kelvin = kelvin.ToString() };
        SendPostRequest(uri, body);
    }

    #endregion
}

/// <summary>
/// Represents an entity ID.
/// </summary>
[Serializable]
public class EntityID
{
    /// <summary>
    /// The entity ID.
    /// </summary>
    public string entity_id;
}

/// <summary>
/// Represents an entity ID.
/// </summary>
[Serializable]
public class Brightness
{
    /// <summary>
    /// The entity ID.
    /// </summary>
    public string entity_id;
    public string brightness = null;
}

/// <summary>
/// Represents an entity ID.
/// </summary>
[Serializable]
public class Kelvin
{
    /// <summary>
    /// The entity ID.
    /// </summary>
    public string entity_id;
    public string kelvin = null;
}

/// <summary>
/// Represents an entity ID.
/// </summary>
[Serializable]
public class RGBColor
{
    /// <summary>
    /// The entity ID.
    /// </summary>
    public string entity_id;
    public int[] rgb_color;
}