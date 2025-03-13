using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Utils;

namespace Managers
{
    /// <summary>
    /// Handles the UI stuff like tab switching and updating the header text.
    /// </summary>
    public class UIManager : Singleton<UIManager>
    {
        [SerializeField] private TMP_Text HeaderText;

        [SerializeField] private GameObject MainUI;
        [SerializeField] private GameObject HomeButtonUI;
        [SerializeField] private Button HomeButton;
        [SerializeField] private Button CloseMainUIButton;
        private LazyFollow _mainUILazyFollow;
        
        [Header("Tabs")]
        [SerializeField] private GameObject OverviewTab;
        [SerializeField] private GameObject NewDeviceTab;
        [SerializeField] private GameObject SettingsTab;
        
        [Header("Sidebar")]
        [SerializeField] private Button OverviewButton;
        [SerializeField] private Button NewDeviceButton;
        [SerializeField] private Button SettingsButton;

        private void OnEnable()
        {
            HideMainMenu();
            OverviewButton.onClick.AddListener(ShowOverviewTab);
            NewDeviceButton.onClick.AddListener(ShowNewDeviceTab);
            SettingsButton.onClick.AddListener(ShowSettingsTab);
            HomeButton.onClick.AddListener(OnHomeButtonClicked);
            CloseMainUIButton.onClick.AddListener(HideMainMenu);
        }

        private void Start()
        {
            HideMainMenu();
        }

        private void OnDisable()
        {
            OverviewButton.onClick.RemoveListener(ShowOverviewTab);
            NewDeviceButton.onClick.RemoveListener(ShowNewDeviceTab);
            SettingsButton.onClick.RemoveListener(ShowSettingsTab);
            HomeButton.onClick.RemoveListener(OnHomeButtonClicked);
            CloseMainUIButton.onClick.RemoveListener(HideMainMenu);
        }

        private void OnHomeButtonClicked()
        {
            ShowOverviewTab();
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
        /// Shows the specified tab and hides all other tabs.
        /// Also makes sure the main UI is visible and the home button is hidden.
        /// </summary>
        /// <param name="tab">The tab to show.</param>
        private void ShowTab(GameObject tab)
        {
            // Hide all other tabs
            OverviewTab.SetActive(false);
            SettingsTab.SetActive(false);
            NewDeviceTab.SetActive(false);
            
            // Show the selected tab
            tab.SetActive(true);
            
            // Show the main UI and hide the home button
            MainUI.SetActive(true);
            HomeButtonUI.SetActive(false);
            
            // Disable lazy follow while the main UI is open
            if (_mainUILazyFollow == null)
                _mainUILazyFollow = MainUI.GetComponentInParent<LazyFollow>();
            if (_mainUILazyFollow != null)
                _mainUILazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.None;
        }

        /// <summary>
        /// Hides the main UI and shows the home button.
        /// Also activates the position follow mode of the LazyFollow.
        /// </summary>
        private void HideMainMenu()
        {
            MainUI.SetActive(false);
            HomeButtonUI.SetActive(true);
            
            if (_mainUILazyFollow == null)
                _mainUILazyFollow = MainUI.GetComponentInParent<LazyFollow>();
            if (_mainUILazyFollow != null)
                _mainUILazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
        }

        public void CloseMainMenu()
        {
            HideMainMenu();
        }
    }
}