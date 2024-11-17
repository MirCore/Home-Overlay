using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace UI
{
    public class NewDeviceUI : MonoBehaviour
    {
        /// <summary>
        /// The TMP_Dropdown used to select the device type.
        /// </summary>
        [SerializeField] private TMP_Dropdown TypeDropdown;
        
        /// <summary>
        /// The TMP_Dropdown used to select the entity.
        /// </summary>
        [SerializeField] private TMP_Dropdown EntityDropdown;
        /// <summary>
        /// The DropdownItem that shows the currently selected entity.
        /// </summary>
        [SerializeField] private DropdownItem SelectedEntityLabel;
        
        /// <summary>
        /// The Button that creates a new entity.
        /// </summary>
        [SerializeField] private Button CreateEntityButton;
        
        /// <summary>
        /// The EntitySpawner that spawns new entities.
        /// </summary>
        [SerializeField] private EntitySpawner EntitySpawner;
        
        /// <summary>
        /// The currently selected device type.
        /// </summary>
        private EDeviceType _selectedEDeviceType;


        private void OnEnable()
        {
            GetHassEntities();
            
            TypeDropdown.onValueChanged.AddListener(OnTypeDropdownValueChanged);
            EntityDropdown.onValueChanged.AddListener(OnEntityDropdownValueChanged);
            CreateEntityButton.onClick.AddListener(OnCreateEntityButtonClicked);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        private void OnDisable()
        {
            TypeDropdown.onValueChanged.RemoveListener(OnTypeDropdownValueChanged);
            EntityDropdown.onValueChanged.RemoveListener(OnEntityDropdownValueChanged);
            CreateEntityButton.onClick.RemoveListener(OnCreateEntityButtonClicked);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }
        
        private void Start()
        {
            UpdateTypeDropdown();
            UpdateEntityDropdown();
        }

        /// <summary>
        /// Updates the options of the TypeDropdown.
        /// </summary>
        private void UpdateTypeDropdown()
        {
            TypeDropdown.ClearOptions();
            TypeDropdown.AddOptions(Enum.GetValues(typeof(EDeviceType)).Cast<EDeviceType>().Select(e => e.GetDisplayName()).ToList());
        }

        /// <summary>
        /// Handles the click event of the CreateEntityButton.
        /// Spawns a new entity at the position of the CreateEntityButton with the selected entity ID.
        /// </summary>
        private void OnCreateEntityButtonClicked()
        {
            // Get the selected entity ID from the EntityDropdown
            string selectedEntityID = EntityDropdown.options[EntityDropdown.value].text;

            // Spawn a new entity at the position of the CreateEntityButton with the selected entity ID
            EntitySpawner.SpawnEntity(selectedEntityID, CreateEntityButton.transform);
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
        /// Updates the EntityDropdown based on the selected device type.
        /// 
        /// If the selected device type is DEFAULT, all entities are shown.
        /// Otherwise, only entities with the selected device type are shown.
        /// </summary>
        private void UpdateEntityDropdown()
        {
            EntityDropdown.ClearOptions();

            // Get a list of all the entities with the selected device type
            List<string> subtitleList = new ();
            foreach (KeyValuePair<string, HassEntity> entity in GameManager.Instance.HassStates)
            {
                // Skip entities with device type DEFAULT, as these are not compatible
                if (entity.Value.DeviceType == EDeviceType.DEFAULT)
                    continue;
                
                // Add the entity ID to the list if it matches the selected device type
                if (_selectedEDeviceType == EDeviceType.DEFAULT || entity.Value.DeviceType == _selectedEDeviceType)
                    subtitleList.Add(entity.Key);
            }

            // Add the entity IDs to the dropdown
            EntityDropdown.AddOptions(subtitleList);

            // Update the selected entity label
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
    }
}
