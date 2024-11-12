using System.Collections.Generic;
using System.IO;
using Models;
using Proyecto26;
using UnityEditor;
using UnityEngine;

public class RestHandler : MonoBehaviour
{
    private void Start()
    {
        string url = GameManager.Instance.HassURL;
        string token = GameManager.Instance.Token;
        
        RequestHelper get = new()
        {
            Uri = url,
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            }
        };
        RequestHelper post = new()
        {
            Uri = Path.Combine(url, "services/input_boolean/toggle"),
            Headers = new Dictionary<string, string>
            {
                {"Authorization", $"Bearer {token}"},
                {"content-type", "application/json"}
            },
            Body = new EntityID{ entity_id = "input_boolean.toggle"}
        };
        /*RestClient.Get(get).Then(response => {
            EditorUtility.DisplayDialog("Response", response.Text, "Ok");
        });*/
        RestClient.Post(post).Then(response => {
                Debug.Log("Response: " + response.Text);
        })
        .Catch(err => {
            Debug.LogError("Error: " + err.Message);
        });
    }
}

[System.Serializable]
public class EntityID
{
    // ReSharper disable once InconsistentNaming
    public string entity_id;
}