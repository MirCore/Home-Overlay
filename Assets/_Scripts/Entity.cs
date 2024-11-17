using Managers;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class Entity : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private SVGImage Icon;
    private string _entityID;
    private HassEntity _entityState;

    private void OnEnable()
    {
        Button.onClick.AddListener(OnButtonClicked);
        EventManager.OnHassStatesChanged += OnHassStatesChanged;
    }

    private void OnDisable()
    {
        Button.onClick.RemoveListener(OnButtonClicked);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    /// <summary>
    /// Called when Hass states change. Updates the icon.
    /// </summary>
    private void OnHassStatesChanged()
    {
        UpdateIcon();
    }

    /// <summary>
    /// Updates the icon color based on the entity's state and attributes.
    /// </summary>
    private void UpdateIcon()
    {
        // If there is no entity ID, there is nothing to update.
        if (_entityID == null)
            return;
        
        // Get the current state of the entity.
        _entityState = GameManager.Instance.HassStates[_entityID];
        if (_entityState == null)
            return;

        // Update the icon color based on the entity's state.
        // If the entity is off, set the icon color to black.
        if (_entityState.state == "off")
        {
            Icon.color = Color.black;
        }
        // If the entity has a valid RGB color, set the icon color to it.
        else if (_entityState.attributes.rgb_color is { Length: 3 })
        {
            Icon.color = JsonHelpers.RGBToUnityColor(_entityState.attributes.rgb_color);
        }
        // Otherwise, set the icon color to white.
        else
        {
            Icon.color = Color.white;
        }
    }

    /// <summary>
    /// Toggles the light state associated with the entity ID.
    /// </summary>
    private void OnButtonClicked()
    {
        RestHandler.ToggleLight(_entityID);
    }

    /// <summary>
    /// Sets the entity ID and updates the icon accordingly.
    /// </summary>
    /// <param name="selectedEntityID">The selected entity ID.</param>
    public void SetEntityID(string selectedEntityID)
    {
        _entityID = selectedEntityID;
        UpdateIcon();
    }
}