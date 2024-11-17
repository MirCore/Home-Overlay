using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Managers
{
    /// <summary>
    /// Handles the UI stuff like tab switching and updating the header text.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private TMP_Text HeaderText;
        [SerializeField] private GameObject NewDeviceTab;
        [SerializeField] private GameObject SettingsTab;
        [SerializeField] private Button NewDeviceButton;
        [SerializeField] private Button SettingsButton;

        private void Start()
        {
            ShowNewDeviceTab();
        }

        private void OnEnable()
        {
            NewDeviceButton.onClick.AddListener(ShowNewDeviceTab);
            SettingsButton.onClick.AddListener(ShowSettingsTab);
        }

        private void OnDisable()
        {
            NewDeviceButton.onClick.RemoveListener(ShowNewDeviceTab);
            SettingsButton.onClick.RemoveListener(ShowSettingsTab);
        }
        
        /// <summary>
        /// Shows the NewDeviceTab and hides the SettingsTab.
        /// Updates the header text to "Add new Device".
        /// </summary>
        private void ShowNewDeviceTab()
        {
            NewDeviceTab.SetActive(true);
            SettingsTab.SetActive(false);
            
            HeaderText.text = "Add new Device";
        }
        
        /// <summary>
        /// Shows the SettingsTab and hides the NewDeviceTab.
        /// Updates the header text to "Settings".
        /// </summary>
        private void ShowSettingsTab()
        {
            NewDeviceTab.SetActive(false);
            SettingsTab.SetActive(true);
            
            HeaderText.text = "Settings";
        }
    
    }

}