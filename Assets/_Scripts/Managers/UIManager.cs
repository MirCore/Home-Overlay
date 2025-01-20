using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Managers
{
    /// <summary>
    /// Handles the UI stuff like tab switching and updating the header text.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private TMP_Text HeaderText;
        [SerializeField] private GameObject OverviewTab;
        [SerializeField] private GameObject NewDeviceTab;
        [SerializeField] private GameObject SettingsTab;
        [SerializeField] private Button OverviewButton;
        [SerializeField] private Button NewDeviceButton;
        [SerializeField] private Button SettingsButton;

        private void OnEnable()
        {
            ShowOverviewTab();
            OverviewButton.onClick.AddListener(ShowOverviewTab);
            NewDeviceButton.onClick.AddListener(ShowNewDeviceTab);
            SettingsButton.onClick.AddListener(ShowSettingsTab);
        }

        private void Start()
        {
            ShowOverviewTab();
        }

        private void OnDisable()
        {
            OverviewButton.onClick.RemoveListener(ShowOverviewTab);
            NewDeviceButton.onClick.RemoveListener(ShowNewDeviceTab);
            SettingsButton.onClick.RemoveListener(ShowSettingsTab);
        }

        /// <summary>
        /// Shows the OverviewTab and hides the other tabs.
        /// Updates the header text to "Overview".
        /// </summary>
        private void ShowOverviewTab()
        {
            ShowTab(OverviewTab);
            
            HeaderText.text = "Overview";
        }

        /// <summary>
        /// Shows the NewDeviceTab and hides the NewDeviceTab.
        /// Updates the header text to "Add new Device".
        /// </summary>
        private void ShowNewDeviceTab()
        {
            ShowTab(NewDeviceTab);
            
            HeaderText.text = "Add new Device";
        }
        
        /// <summary>
        /// Shows the SettingsTab and hides the SettingsTab.
        /// Updates the header text to "Settings".
        /// </summary>
        internal void ShowSettingsTab()
        {
            ShowTab(SettingsTab);
            
            HeaderText.text = "Settings";
        }

        /// <summary>
        /// Shows the specified tab and hides the other tabs.
        /// </summary>
        /// <param name="tab">The tab to show.</param>
        private void ShowTab(GameObject tab)
        {
            OverviewTab.SetActive(false);
            SettingsTab.SetActive(false);
            NewDeviceTab.SetActive(false);
            
            tab.SetActive(true);
        }
    
    }

}