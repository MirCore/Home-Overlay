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
    private static RequestHelper _getRequestHelper;
    private static DateTime _lastHassStateRefresh;

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
            EventManager.InvokeOnConnectionTested((int)response.StatusCode, uri);
        })
        .Catch(err => {
            if (err is RequestException requestException)
            {
                int statusCode = (int)requestException.StatusCode;
                EventManager.InvokeOnConnectionTested(statusCode, uri);
            }
            else
            {
                EventManager.InvokeOnConnectionTested(err.HResult, uri);
                Debug.LogError("Error: " + err.Message + "\n" + err.StackTrace);
            }
        });
    }

    #region Get

    public static void GetHassConfig()
    {
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
    
    private static void SendGetRequest(Uri uri, Action<string> action = null)
    {
        _getRequestHelper ??= new RequestHelper { Uri = uri.ToString() };
        _getRequestHelper.Uri = uri.ToString();
        
        RestClient.Get(_getRequestHelper)
            .Then(response => HandleResponse(action, response))
            .Catch(err => {
                Debug.LogError("Error: " + err.Message + err.StackTrace + " Uri: " + uri);
            });
    }

    /// <summary>
    /// Gets the Home Assistant entities.
    /// </summary>
    public static void GetHassEntities()
    {
        if (HassStatesUpdatedRecently())
            return;
        if (!RestClient.DefaultRequestHeaders.ContainsKey("Authorization"))
            return;
        if (GameManager.Instance.HassUri == null)
            return;
        
        Uri uri = new (GameManager.Instance.HassUri, "states");
        SendGetRequest(uri);
    }


    public static void GetCalendar(string entityID, Action<string> updateCalendar)
    {
        string start = DateTime.Now.ToString("yyyy-MM-ddT00:00:00.000Z");
        string end = DateTime.Now.AddDays(31).ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
        Uri uri = new (GameManager.Instance.HassUri, $"calendars/{entityID}?start={Uri.EscapeDataString(start)}&end={Uri.EscapeDataString(end)}");       
        SendGetRequest(uri, updateCalendar);
    }
    
    #endregion

    #region Post

    /// <summary>
    /// Sends a POST request to the specified URI with the given panel ID.
    /// </summary>
    /// <param name="uri">The URI to send the POST request to.</param>
    /// <param name="body">The data to include in the request body.</param>
    /// <param name="action">[Optional] The action to perform with the response text.</param>
    private static void SendPostRequest(Uri uri, object body, Action<string> action = null)
    {
        RequestHelper postRequest = new()
        {
            Uri = uri.ToString(),
            Body = body
        };
        
        RestClient.Post(postRequest)
            .Then(response => HandleResponse(action, response))
            .Catch(err => {
                Debug.LogError("Error: " + err.Message + err.StackTrace);
            });
    }

    /// <summary>
    /// Toggles the light state for the specified panel ID.
    /// </summary>
    /// <param name="entityID">The panel ID of the light to toggle.</param>
    public static void ToggleLight(string entityID)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/light/toggle");
        EntityID body = new() { entity_id = entityID };
        SendPostRequest(uri, body);
    }

    /// <summary>
    /// Toggles the switch state for the specified panel ID.
    /// </summary>
    /// <param name="entityID">The panel ID of the switch to toggle.</param>
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
        int[] rgb = { (int)(color.r * 255), (int)(color.g * 255), (int)(color.b * 255) };
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

    public static void GetWeatherForecast(string entityID, Action<string> entityWeather)
    {
        Uri uri = new (GameManager.Instance.HassUri, "services/weather/get_forecasts?return_response=true");
        GetForecast body = new() { entity_id = entityID, type = "daily" };
        SendPostRequest(uri, body, entityWeather);
    }
    
    #endregion

    private static void HandleResponse(Action<string> action, ResponseHelper response)
    {
        if (action == null)
        {
            HassStates.OnHassStatesResponse(response.Text);
            _lastHassStateRefresh = DateTime.Now;
        }
        else
            action.Invoke(response.Text);

#if UNITY_EDITOR
        if (GameManager.Instance.DebugLogPostResponses)
            Debug.Log(response.Text);
#endif
    }

    private static bool HassStatesUpdatedRecently()
    {
        return _lastHassStateRefresh.AddSeconds((float)GameManager.Instance.HassStateRefreshRate / 2) > DateTime.Now;
    }
    
    #region Json Data Classes
    
    /// <summary>
    /// Represents the data to be sent in a POST request.
    /// </summary>
    [Serializable]
    private class EntityID
    {
        /// <summary>
        /// The panel ID.
        /// </summary>
        public string entity_id;
    }

    /// <summary>
    /// Represents the data to be sent in a POST request.
    /// </summary>
    [Serializable]
    private class Brightness
    {
        /// <summary>
        /// The panel ID.
        /// </summary>
        public string entity_id;
        public string brightness = null;
    }

    /// <summary>
    /// Represents the data to be sent in a POST request.
    /// </summary>
    [Serializable]
    private class Kelvin
    {
        /// <summary>
        /// The panel ID.
        /// </summary>
        public string entity_id;
        public string kelvin = null;
    }

    /// <summary>
    /// Represents the data to be sent in a POST request.
    /// </summary>
    [Serializable]
    private class RGBColor
    {
        /// <summary>
        /// The panel ID.
        /// </summary>
        public string entity_id;
        public int[] rgb_color;
    }

    /// <summary>
    /// Represents the data to be sent in a POST request.
    /// </summary>
    [Serializable]
    private class GetForecast
    {
        /// <summary>
        /// The panel ID.
        /// </summary>
        public string entity_id;
        public string type;
    }
    
    #endregion
}