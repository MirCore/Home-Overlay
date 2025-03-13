using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
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
    /// The Button that creates a new panel.
    /// </summary>
    [Header("Buttons")]
    [SerializeField] private Button CreatePanelButton;
    
    /// <summary>
    /// The Button that saves the settings.
    /// </summary>
    [SerializeField] private Button SaveChangesButton;
    
    /// <summary>
    /// The Button that cancels the changes.
    /// </summary>
    [SerializeField] private Button CancelChangesButton;
    
    /// <summary>
    /// The PanelSpawner that spawns new entities.
    /// </summary>
    [FormerlySerializedAs("EntitySpawner")]
    [Header("Managers")]
    [SerializeField] private PanelSpawner PanelSpawner;
    
    /// <summary>
    /// The currently selected device type.
    /// </summary>
    private EDeviceType _selectedEDeviceType;
    private string _selectedEntityID;
    private List<string> _entityIDList = new ();

    private Panel.Panel _panel;
    private string _searchText = "";

    private void OnEnable()
    {
        UpdateTypeDropdown();
        GetHassEntities();
        GenerateEntityList();
        
        SearchInputField.onValueChanged.AddListener(OnSearchInputFieldValueChanged);
        
        if (TypeDropdown)
            TypeDropdown.onValueChanged.AddListener(OnTypeDropdownValueChanged);
        if (CreatePanelButton)
            CreatePanelButton.onClick.AddListener(OnCreatePanelButtonClicked);
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
        if (CreatePanelButton)
            CreatePanelButton.onClick.RemoveListener(OnCreatePanelButtonClicked);
        if (SaveChangesButton)
            SaveChangesButton.onClick.RemoveListener(OnSaveChangesButtonClicked);
        if (CancelChangesButton)
            CancelChangesButton.onClick.RemoveListener(OnCancelChangesButtonClicked);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    #region Apply or Cancel Changes
    
    private void OnSaveChangesButtonClicked()
    {
        _panel.AssignNewEntityID(_selectedEntityID);
    }

    private void OnCancelChangesButtonClicked()
    {
        _panel.ReloadSettingsWindow();
    }
    
    /// <summary>
    /// Handles the click event of the CreatePanelButton.
    /// Spawns a new panel at the position of the CreatePanelButton with the selected panel ID.
    /// </summary>
    private void OnCreatePanelButtonClicked()
    {
        // Get the selected panel ID from the EntityDropdown
        string selectedEntityID = _selectedEntityID;

        // Spawn a new panel at the position of the CreatePanelButton with the selected panel ID
        PanelSpawner.SpawnNewEntity(selectedEntityID, CreatePanelButton.transform.position);

        UIManager.Instance.CloseMainMenu();
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
        // Get the filtered list of entityIDs
        List<string> entityIDList = GetFilteredEntityIDList();
        
        // Check if the list of entityIDs has changed, if not, return
        if (entityIDList.SequenceEqual(_entityIDList))
            return;
        
        // Return all the panels to the pool
        ObjectPool.Instance.ReturnObjectsToPool(_entityPanels);

        // Update the list of entityIDs
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
    /// Populates the EntityPanel with the given list of entityIDs.
    /// </summary>
    /// <param name="entityIDList">The list of entityIDs to be added to the panel.</param>
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
            
            // Highlight the selected panel if it is the same as the selected entityID
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
    /// Sets the Panel this EntityPicker is part of. Also selects the entity in the dropdown
    /// </summary>
    /// <param name="panel"></param>
    public void SetEntity(Panel.Panel panel)
    {
        _panel = panel;
        _selectedEntityID = _panel.PanelData.EntityID;
        TypeDropdown.value = (int)_panel.GetDeviceType();
    }
}
