using System;
using System.Collections.Generic;
using System.Linq;
using Managers;
using Proyecto26;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using Utils;

namespace UI
{
    public class NewDeviceUI : MonoBehaviour
    {
        [SerializeField] private TMP_Dropdown TypeDropdown;
        [SerializeField] private TMP_Dropdown EntityDropdown;
        [SerializeField] private DropdownItem SelectedEntityLabel;
        private EDeviceType _selectedEDeviceType;

        private void Start()
        {
            TypeDropdown.ClearOptions();
            TypeDropdown.AddOptions(Enum.GetValues(typeof(EDeviceType)).Cast<EDeviceType>().Select(e => e.GetDisplayName()).ToList());
            UpdateEntityDropdown();
        }

        private void OnEnable()
        {
            GetHassEntities();
            
            TypeDropdown.onValueChanged.AddListener(OnTypeDropdownValueChanged);
            EntityDropdown.onValueChanged.AddListener(OnEntityDropdownValueChanged);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        private void OnDisable()
        {
            TypeDropdown.onValueChanged.RemoveListener(OnTypeDropdownValueChanged);
            EntityDropdown.onValueChanged.RemoveListener(OnEntityDropdownValueChanged);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        private void OnEntityDropdownValueChanged(int arg0)
        {
            SelectedEntityLabel.UpdateTitle();
        }

        private void OnHassStatesChanged()
        {
            UpdateEntityDropdown();
        }

        private void UpdateEntityDropdown()
        {
            EntityDropdown.ClearOptions();
            List<string> subtitleList = new ();
            foreach (HassEntity entity in GameManager.Instance.HassStates)
            {
                if (entity.DeviceType == EDeviceType.DEFAULT)
                    continue;

                if (_selectedEDeviceType == EDeviceType.DEFAULT || entity.DeviceType == _selectedEDeviceType)
                    subtitleList.Add(entity.entity_id);
            }

            EntityDropdown.AddOptions(subtitleList);
            SelectedEntityLabel.UpdateTitle();
        }

        private void OnTypeDropdownValueChanged(int index)
        {
            SelectedEntityLabel.UpdateTitle();
            _selectedEDeviceType = (EDeviceType)index;
            UpdateEntityDropdown();
        }

        private void GetHassEntities()
        {
            RestHandler.GetHassEntities();
        }
    }
}
