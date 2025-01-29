using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class EntityPicker : MonoBehaviour
{
    /// <summary>
    /// The TMP_Dropdown used to select the device type.
    /// </summary>
    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown TypeDropdown;
        
    /// <summary>
    /// The TMP_Dropdown used to select the entity.
    /// </summary>
    [SerializeField] private TMP_Dropdown EntityDropdown;
    /// <summary>
    /// The FriendlyNameHandler that shows the currently selected entity.
    /// </summary>
    [SerializeField] private FriendlyNameHandler SelectedEntityLabel;
    
    /// <summary>
    /// The Button that creates a new entity.
    /// </summary>
    [Header("Buttons")]
    [SerializeField] private Button CreateEntityButton;
    
    /// <summary>
    /// The Button that saves the settings.
    /// </summary>
    [SerializeField] private Button SaveChangesButton;
    
    /// <summary>
    /// The Button that cancels the changes.
    /// </summary>
    [SerializeField] private Button CancelChangesButton;
    
    /// <summary>
    /// The EntitySpawner that spawns new entities.
    /// </summary>
    [Header("Managers")]
    [SerializeField] private EntitySpawner EntitySpawner;
    
    /// <summary>
    /// The currently selected device type.
    /// </summary>
    private EDeviceType _selectedEDeviceType;

    private Entity _entity;
    
    private void OnEnable()
    {
        UpdateTypeDropdown();
        GetHassEntities();
        UpdateEntityDropdown();
        
        if (TypeDropdown)
            TypeDropdown.onValueChanged.AddListener(OnTypeDropdownValueChanged);
        if (EntityDropdown)
            EntityDropdown.onValueChanged.AddListener(OnEntityDropdownValueChanged);
        if (CreateEntityButton)
            CreateEntityButton.onClick.AddListener(OnCreateEntityButtonClicked);
        if (SaveChangesButton)
            SaveChangesButton.onClick.AddListener(OnSaveChangesButtonClicked);
        if (CancelChangesButton)
            CancelChangesButton.onClick.AddListener(OnCancelChangesButtonClicked);
        EventManager.OnHassStatesChanged += OnHassStatesChanged;
    }

    private void OnDisable()
    {
        if (TypeDropdown)
            TypeDropdown.onValueChanged.RemoveListener(OnTypeDropdownValueChanged);
        if (EntityDropdown)
            EntityDropdown.onValueChanged.RemoveListener(OnEntityDropdownValueChanged);
        if (CreateEntityButton)
            CreateEntityButton.onClick.RemoveListener(OnCreateEntityButtonClicked);
        if (SaveChangesButton)
            SaveChangesButton.onClick.RemoveListener(OnSaveChangesButtonClicked);
        if (CancelChangesButton)
            CancelChangesButton.onClick.RemoveListener(OnCancelChangesButtonClicked);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    #region Apply or Cancel Changes
    
    private void OnSaveChangesButtonClicked()
    {
        // Get the selected entity ID from the EntityDropdown
        string selectedEntityID = GetSelectedEntityID();
            
        _entity.UpdateEntityID(selectedEntityID);
    }

    private void OnCancelChangesButtonClicked()
    {
        _entity.ReloadSettingsWindow();
    }
    
    /// <summary>
    /// Handles the click event of the CreateEntityButton.
    /// Spawns a new entity at the position of the CreateEntityButton with the selected entity ID.
    /// </summary>
    private void OnCreateEntityButtonClicked()
    {
        // Get the selected entity ID from the EntityDropdown
        string selectedEntityID = GetSelectedEntityID();

        // Spawn a new entity at the position of the CreateEntityButton with the selected entity ID
        EntitySpawner.SpawnNewEntity(selectedEntityID, CreateEntityButton.transform.position);
    }

    #endregion

    #region Dropdown Behavior
    
    /// <summary>
    /// Updates the options of the TypeDropdown.
    /// </summary>
    private void UpdateTypeDropdown()
    {
        TypeDropdown.ClearOptions();
        TypeDropdown.AddOptions(Enum.GetValues(typeof(EDeviceType)).Cast<EDeviceType>().Select(e => e.GetDisplayName()).ToList());
    }

    /// <summary>
    /// Updates the EntityDropdown based on the selected device type.
    /// If the selected device type is DEFAULT, all entities are shown.
    /// Otherwise, only entities with the selected device type are shown.
    /// </summary>
    private void UpdateEntityDropdown()
    {
        string selectedEntity = GetSelectedEntityID();
        EntityDropdown.ClearOptions();

        // Get a list of all the entities with the selected device type
        List<string> entityIDList = new ();
        foreach (KeyValuePair<string, HassEntity> entity in HassStates.GetHassStates())
        {
            // Skip entities with device type DEFAULT, as these are not compatible
            if (entity.Value.DeviceType == EDeviceType.DEFAULT)
                continue;
                
            // Add the entity ID to the list if it matches the selected device type
            if (_selectedEDeviceType == EDeviceType.DEFAULT || entity.Value.DeviceType == _selectedEDeviceType)
                entityIDList.Add(entity.Key);
        }

        // Add the entity IDs to the dropdown
        EntityDropdown.AddOptions(entityIDList);
        
        SetEntityDropdownToEntityID(selectedEntity);

        // Update the selected entity label
        SelectedEntityLabel.UpdateTitle();
    }

    private void SetEntityDropdownToEntityID(string entityID)
    {
        int entityIndex = EntityDropdown.options.FindIndex(option => option.text == entityID);
        EntityDropdown.value = entityIndex;
    }

    /// <summary>
    /// Handles the change in the EntityDropdown selection.
    /// Updates the selected entity ID and the title of the SelectedEntityLabel.
    /// </summary>
    /// <param name="index">The index of the selected item in the EntityDropdown.</param>
    private void OnEntityDropdownValueChanged(int index)
    {
        SelectedEntityLabel.UpdateTitle();
    }
    
    /// <summary>
    /// Handles the change in the TypeDropdown selection.
    /// Updates the selected device type and refreshes the entity dropdown accordingly.
    /// </summary>
    /// <param name="index">The index of the selected item in the TypeDropdown.</param>
    private void OnTypeDropdownValueChanged(int index)
    {
        // Update the selected device type based on the dropdown index
        _selectedEDeviceType = (EDeviceType)index;
            
        // Refresh the entity dropdown to reflect the new device type selection
        UpdateEntityDropdown();
    }

    private string GetSelectedEntityID()
    {
        if (EntityDropdown.options.Count == 0)
            return string.Empty;
        return EntityDropdown.options[EntityDropdown.value].text;
    }

    #endregion
    
    /// <summary>
    /// Gets the Home Assistant Entities via the RestHandler.
    /// The result is then passed to the OnHassStatesChanged method.
    /// </summary>
    private static void GetHassEntities()
    {
        RestHandler.GetHassEntities();
    }

    /// <summary>
    /// Updates the EntityDropdown when the HassStates are changed
    /// </summary>
    private void OnHassStatesChanged()
    {
        UpdateEntityDropdown();
    }

    public void SetEntity(Entity entity)
    {
        _entity = entity;
        TypeDropdown.value = (int)_entity.GetDeviceType();
        SetEntityDropdownToEntityID(_entity.EntityObject.EntityID);
    }
}
