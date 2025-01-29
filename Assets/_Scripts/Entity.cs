using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using Utils;

public class Entity : MonoBehaviour, IDragHandler
{
    [SerializeField] private Button Button;
    [SerializeField] private TMP_Text Icon;
    [SerializeField] private Button SettingsButton;
    private XRBaseInteractable _interactable;
    
    private HassEntity _entityState;
    public EntityObject EntityObject { get; private set; }

    private ARAnchorManager _arAnchorManager;
    private ARAnchor _anchor;
    
    /// <summary>
    /// A coroutine that is currently setting the color of the button temporarily.
    /// </summary>
    private IEnumerator _setButtonColorTemporarilyCoroutine;

    private void OnEnable()
    {
        _arAnchorManager = GameManager.Instance.ARAnchorManager;
        _arAnchorManager.trackablesChanged.AddListener(OnAnchorChanged);
        
        Button.onClick.AddListener(OnButtonClicked);
        SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
        _interactable = GetComponent<XRBaseInteractable>();
        IDragHandler foo = GetComponent<IDragHandler>();
        _interactable.selectExited.AddListener(OnSelectExited);
        EventManager.OnHassStatesChanged += OnHassStatesChanged;
    }

    private void OnDisable()
    {
        _arAnchorManager.trackablesChanged.RemoveListener(OnAnchorChanged);
        
        Button.onClick.RemoveListener(OnButtonClicked);
        SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
        _interactable.selectExited.RemoveListener(OnSelectExited);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    /// <summary>
    /// Listens for when AR anchors are added to the scene, and parents this entity to the first anchor that matches the EntityObject's AnchorID.
    /// </summary>
    private void OnAnchorChanged(ARTrackablesChangedEventArgs<ARAnchor> changes)
    {
        if (_anchor != null)
            return;
        
        foreach (ARAnchor addedAnchor in changes.added.Where(addedAnchor => EntityObject.AnchorID == addedAnchor.trackableId.ToString()))
        {
            SetParentToAnchor(addedAnchor);
            Debug.Log("Entity parented to added anchor");
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (XRSettings.enabled)
            return;
        
        CreateAnchorAsync();
        Debug.Log("ondrag");
        
        Canvas canvas = GetComponent<Canvas>();
        RectTransform rectTransform = transform as RectTransform;
        if (canvas != null && rectTransform != null)
        {
            float scaleFactor = rectTransform.localScale.magnitude;
            Vector2 adjustedDelta = eventData.delta * scaleFactor;

            rectTransform.anchoredPosition += adjustedDelta; // Adjust anchoredPosition
            if (EntityObject == null)
                return;
            EntityObject.Position = transform.localPosition;
        }
        
    }
    
    private void OnSelectExited(SelectExitEventArgs eventData)
    {
        if (EntityObject == null)
            return;

        CreateAnchorAsync();

        EntityObject.Position = transform.position;
    }

    /// <summary>
    /// Sets the parent of the entity to the specified ARAnchor.
    /// </summary>
    /// <param name="newAnchor">The new anchor to attach to.</param>
    private void SetParentToAnchor(ARAnchor newAnchor)
    {
        _anchor = newAnchor;
        // Set the entity's parent to the anchor
        transform.SetParent(_anchor.transform, false);
        // Align the entity's position with the anchor
        transform.position = _anchor.transform.position;
        // Update the AnchorID in the entity object
        EntityObject.AnchorID = _anchor.trackableId.ToString();
        //Debug.Log("Attached to anchor: " + EntityObject.AnchorID);
    }

    /// <summary>
    /// Asynchronously creates an ARAnchor at the entity's current pose.
    /// </summary>
    private async void CreateAnchorAsync()
    {
        // Attempt to add a new anchor at the current position and rotation
        Result<ARAnchor> result = await _arAnchorManager.TryAddAnchorAsync(new Pose(transform.position, transform.rotation));
        if (!result.status.IsSuccess())
        {
            // Log a warning if the anchor creation fails
            Debug.LogWarning("Failed to create anchor");
            return;
        }

        ARAnchor oldAnchor = _anchor; // Store reference to the old anchor
        ARAnchor newAnchor = result.value; // Get the newly created anchor

        SetParentToAnchor(newAnchor); // Re-parent the entity to the new anchor

        if (oldAnchor != null)
        {
            // Remove the old anchor if it exists
            bool deleted = _arAnchorManager.TryRemoveAnchor(oldAnchor);
            //if (deleted)
            //    Debug.Log("Deleted old anchor");
        }
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
    /// Sets the entity object and updates the icon and anchor accordingly.
    /// </summary>
    /// <param name="entityObject">The entity object to be associated with this entity.</param>
    public void SetEntityObject(EntityObject entityObject)
    {
        // Assign the provided entity object to the current entity
        EntityObject = entityObject;

        // Add the entity object to the GameManager's list of entities
        GameManager.Instance.AddEntity(EntityObject, this);
        
        // Update the icon to reflect the current state of the entity
        UpdateIcon();

        // If the entity object does not have an anchor ID, exit the method
        if (string.IsNullOrEmpty(EntityObject.AnchorID))
            return;
        
        // Retrieve the anchor using the anchor ID
        TrackableId trackableId = new(EntityObject.AnchorID);
        ARAnchor anchor = _arAnchorManager.GetAnchor(trackableId);

        // If an anchor is found, set the entity's parent to the anchor
        if (anchor != null)
        {
            SetParentToAnchor(anchor);
        }
        else
        {
            // Log a message if no anchor is found
            Debug.Log("No anchor loaded");
        }
    }

    public void UpdateEntityID(string entityID)
    {
        EntityObject.EntityID = entityID;
        
        UpdateIcon();
        EntitySettingsWindowManager.Instance.UpdateEntitySettingsWindow(this);
    }

    public void DeleteEntity()
    {
        GameManager.Instance.RemoveEntity(EntityObject);
        Destroy(gameObject);
    }


    /// <summary>
    /// Temporarily highlights the entity by changing the color of its button to red.
    /// The highlighting is done by starting a coroutine that changes the color of the button to red
    /// and then waits for the specified duration and then changes the color back to the original color.
    /// </summary>
    public void HighlightEntity()
    {
        if (_setButtonColorTemporarilyCoroutine != null)
            return;

        _setButtonColorTemporarilyCoroutine = SetButtonColorTemporarily(Color.red, 10f);
        StartCoroutine(_setButtonColorTemporarilyCoroutine);
    }
    
    /// <summary>
    /// Temporarily sets the color of the entity's button to the given color for the given duration.
    /// </summary>
    /// <param name="color">The color to set the button to.</param>
    /// <param name="duration">The duration of the color change in seconds.</param>
    private IEnumerator SetButtonColorTemporarily(Color color, float duration)
    {
        // Store the original color of the button
        Color originalColor = Button.targetGraphic.color;
        
        // Change the color of the button to the given color
        Button.targetGraphic.color = color;
        
        // Store the original color block and normal color of the button
        ColorBlock colorBlock = Button.colors;
        Color originalNormalColor = colorBlock.normalColor;
        
        // Change the normal color of the button to a semi-transparent version of the original normal color
        // Apply the changed color block to the button
        colorBlock.normalColor = new Color(originalNormalColor.r, originalNormalColor.g, originalNormalColor.b, 0.5f);
        Button.colors = colorBlock;
        
        // Wait for the specified duration
        yield return new WaitForSeconds(duration);
        
        // Change the color of the button back to the original color
        Button.targetGraphic.color = originalColor;
        
        // Change the normal color of the button back to the original normal color
        // Apply the changed color block to the button
        colorBlock.normalColor = originalNormalColor;
        Button.colors = colorBlock;
        
        // Set the flag to indicate that the coroutine has finished
        _setButtonColorTemporarilyCoroutine = null;
    }

    public EDeviceType GetDeviceType()
    {
        return _entityState.DeviceType;
    }

    public void ReloadSettingsWindow()
    {
        EntitySettingsWindowManager.Instance.UpdateEntitySettingsWindow(this);
    }
}