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
    
    [SerializeField] private TMP_InputField SearchInputField;
    private readonly List<FriendlyNameHandler> _entityPanels = new ();
    [SerializeField] private Transform _entityPanelsParent;
    
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
    private string _selectedEntityID;
    private List<string> _entityIDList = new ();

    private Entity.Entity _entity;
    private string _searchText = "";

    private void OnEnable()
    {
        UpdateTypeDropdown();
        GetHassEntities();
        GenerateEntityList();
        
        SearchInputField.onValueChanged.AddListener(OnSearchInputFieldValueChanged);
        
        if (TypeDropdown)
            TypeDropdown.onValueChanged.AddListener(OnTypeDropdownValueChanged);
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
        SearchInputField.onValueChanged.RemoveListener(OnSearchInputFieldValueChanged);
        
        if (TypeDropdown)
            TypeDropdown.onValueChanged.RemoveListener(OnTypeDropdownValueChanged);
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
        _entity.AssignNewEntityID(_selectedEntityID);
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
        string selectedEntityID = _selectedEntityID;

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
    /// Handles the change in the TypeDropdown selection.
    /// Updates the selected device type and refreshes the entity dropdown accordingly.
    /// </summary>
    /// <param name="index">The index of the selected item in the TypeDropdown.</param>
    private void OnTypeDropdownValueChanged(int index)
    {
        // Update the selected device type based on the dropdown index
        _selectedEDeviceType = (EDeviceType)index;
            
        // Refresh the entity dropdown to reflect the new device type selection
        GenerateEntityList();
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
        GenerateEntityList();
    }

    private void GenerateEntityList()
    {
        // Get the filtered list of entity IDs
        List<string> entityIDList = GetFilteredEntityIDList();
        
        // Check if the list of entity IDs has changed, if not, return
        if (entityIDList.SequenceEqual(_entityIDList))
            return;
        
        // Return all the entity panels to the pool
        ObjectPool.Instance.ReturnObjectsToPool(_entityPanels);

        // Update the list of entity IDs
        _entityIDList = entityIDList;
        
        // Populate the EntityPanel
        PopulateEntityPanel(entityIDList);
    }

    private List<string> GetFilteredEntityIDList()
    {
        // Get a list of all the entities with the selected device type
        List<string> entityIDList = new ();
        foreach (KeyValuePair<string, HassState> state in HassStates.GetHassStates())
        {
            // Skip entities with device type DEFAULT, as these are not compatible
            if (state.Value.DeviceType == EDeviceType.DEFAULT)
                continue;
                
            // Skip entities that do not match the selected device type
            if (_selectedEDeviceType != EDeviceType.DEFAULT && state.Value.DeviceType != _selectedEDeviceType)
                continue;
            
            if (EntityMatchesSearchTerm(state))
                entityIDList.Add(state.Key);
        }

        return entityIDList;
    }
    /// <summary>
    /// Checks if the state matches the current search term.
    /// An state matches if its name or ID starts with the search term.
    /// </summary>
    /// <param name="state">The state to check.</param>
    /// <returns>True if the state matches the search term, false otherwise.</returns>
    private bool EntityMatchesSearchTerm(KeyValuePair<string, HassState> state)
    {
        if (_searchText == "")
            return true;
        
        string eID = state.Key;
        string eName = HassStates.GetHassState(eID).attributes.friendly_name ?? "";
            
        return eName.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase) ||
               eID.Contains(_searchText, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Populates the EntityPanel with the given list of entity IDs.
    /// </summary>
    /// <param name="entityIDList">The list of entity IDs to be added to the panel.</param>
    private void PopulateEntityPanel(List<string> entityIDList)
    {
        // Sort entities by friendly_name (default to entityID if name is null)
        List<string> sortedEntityList = entityIDList
            .OrderBy(entityID => HassStates.GetHassState(entityID)?.attributes.friendly_name ?? entityID.Split('.')[1])
            .ToList();
        
        foreach (string entityID in sortedEntityList)
        {
            // Get a entity panel from the pool and move it to the ScrollRect
            FriendlyNameHandler entityPickerPanel = ObjectPool.Instance.GetPooledObject();
            entityPickerPanel.transform.SetParent(_entityPanelsParent, false);
            
            // Set the entity panel properties
            entityPickerPanel.SetNewEntity(entityID);
            
            // Highlight the selected entity if it is the same as the selected entityID
            if (entityID == _selectedEntityID)
                entityPickerPanel.Highlight();
            
            // Add a click listener to the entity panel
            entityPickerPanel.Button.onClick.AddListener(() => OnEntityButtonClicked(entityID));
            
            // Add the entity panel to the list so it can be returned to the pool later
            _entityPanels.Add(entityPickerPanel);
        }
    }

    private void OnEntityButtonClicked(string entityID)
    {
        _selectedEntityID = entityID;
    }

    private void OnSearchInputFieldValueChanged(string value)
    {
        _searchText = value;
        GenerateEntityList();
    }

    /// <summary>
    /// Sets the Entity this EntityPicker is part of. Also selects the entity in the dropdown
    /// </summary>
    /// <param name="entity"></param>
    public void SetEntity(Entity.Entity entity)
    {
        _entity = entity;
        _selectedEntityID = _entity.EntityObject.EntityID;
        TypeDropdown.value = (int)_entity.GetDeviceType();
    }
}
