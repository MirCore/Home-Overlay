using System;
using System.Linq;
using Managers;
using Structs;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class Entity : MonoBehaviour
{
    [SerializeField] private Button Button;
    [SerializeField] private TMP_Text Icon;
    [SerializeField] private Button SettingsButton;
    
    private HassEntity _entityState;
    public EntityObject EntityObject { get; private set; }

    private void OnEnable()
    {
        Button.onClick.AddListener(OnButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        EventManager.OnHassStatesChanged += OnHassStatesChanged;
    }

    private void OnDisable()
    {
        Button.onClick.RemoveListener(OnButtonClicked);
        SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    private void OnSettingsButtonClicked()
    {
        EntitySettingsWindowManager.Instance.ToggleSettingsWindow(this);
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
        if (EntityObject == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // If there is no entity ID, there is nothing to update.
        if (EntityObject.EntityID == null)
        {
            Icon.text = MaterialDesignIcons.GetIcon(null, EDeviceType.DEFAULT);
            return;
        }
        
        // Get the current state of the entity.
        _entityState = HassStates.GetHassState(EntityObject.EntityID);
        if (_entityState == null)
            return;

        // Update the icon based on the entity's attributes.
        Icon.text = MaterialDesignIcons.GetIcon(_entityState.attributes.icon, _entityState.DeviceType);

        Color color;
        
        // Update the icon color based on the entity's state.
        // If the entity is off, set the icon color to black.
        if (_entityState.state == "off")
        {
            color = Color.black;
        }
        // If the entity has a valid RGB color, set the icon color to it.
        else if (_entityState.attributes.rgb_color is { Length: 3 })
        {
            color = JsonHelpers.RGBToUnityColor(_entityState.attributes.rgb_color);
        }
        // Otherwise, set the icon color to white.
        else
        {
            color = Color.white;
        }

        if (_entityState.attributes.brightness != 0)
        {
            color = Color.Lerp(Color.black, color, _entityState.attributes.brightness / 255f);
        }
        
        Icon.color = color;
    }

    /// <summary>
    /// Toggles the entities state associated with the entity ID.
    /// </summary>
    private void OnButtonClicked()
    {
        if (_entityState == null)
            return;
        
        switch (_entityState.DeviceType)
        {
            case EDeviceType.DEFAULT:
                break;
            case EDeviceType.LIGHT:
                RestHandler.ToggleLight(EntityObject.EntityID);
                break;
            case EDeviceType.SWITCH:
                RestHandler.ToggleSwitch(EntityObject.EntityID);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    /// <summary>
    /// Sets the entity ID and updates the icon accordingly.
    /// </summary>
    /// <param name="entityObject">The selected entityObject.</param>
    public void SetEntityObject(EntityObject entityObject)
    {
        EntityObject = entityObject;
        
        UpdateIcon();
    }

    public void UpdateEntityID(string entityID)
    {
        EntityObject.EntityID = entityID;
        
        UpdateIcon();
    }
}