using TMPro;
using UI;
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
        private CanvasFader _canvasFader;
        
        [Header("Tabs")]
        [SerializeField] private GameObject OverviewTab;
        [SerializeField] private string OverviewTitle;
        [SerializeField] private GameObject NewDeviceTab;
        [SerializeField] private string NewDeviceTitle;
        [SerializeField] private GameObject SettingsTab;
        [SerializeField] private string SettingsTitle;
        
        [Header("Sidebar")]
        [SerializeField] private Button OverviewButton;
        [SerializeField] private Button NewDeviceButton;
        [SerializeField] private Button SettingsButton;


        private void OnEnable()
        {
            HideMainMenu();
            _canvasFader = HomeButtonUI.GetComponent<CanvasFader>();
            if (OverviewButton)
                OverviewButton.onClick.AddListener(ShowOverviewTab);
            if (NewDeviceButton)
                NewDeviceButton.onClick.AddListener(ShowNewDeviceTab);
            if (SettingsButton)
                SettingsButton.onClick.AddListener(ShowSettingsTab);
            if (HomeButton)
                HomeButton.onClick.AddListener(OnHomeButtonClicked);
            if (CloseMainUIButton)
                CloseMainUIButton.onClick.AddListener(HideMainMenu);
        }

        private void Start()
        {
            HideMainMenu();
        }

        private void OnDisable()
        {
            if (OverviewButton)
                OverviewButton.onClick.RemoveListener(ShowOverviewTab);
            if (NewDeviceButton)
                NewDeviceButton.onClick.RemoveListener(ShowNewDeviceTab);
            if (SettingsButton)
                SettingsButton.onClick.RemoveListener(ShowSettingsTab);
            if (HomeButton)
                HomeButton.onClick.RemoveListener(OnHomeButtonClicked);
            if (CloseMainUIButton)
                CloseMainUIButton.onClick.RemoveListener(HideMainMenu);
        }

        private void OnHomeButtonClicked()
        {
#if UNITY_VISIONOS && !UNITY_EDITOR
            SwiftUIDriver.OpenSwiftUIWindow("MainMenu");
            HomeButtonUI.SetActive(false);
            return;
#endif
            if (!MainUI)
                return;
            
            if (Camera.main)
            {
                MainUI.transform.LookAt(Camera.main.transform.position, Vector3.up);
                MainUI.transform.forward = -MainUI.transform.forward;
            }
            ShowOverviewTab();
        }

        /// <summary>
        /// Shows the OverviewTab and hides the other tabs.
        /// Updates the header text to "Overview".
        /// </summary>
        private void ShowOverviewTab()
        {
            ShowTab(OverviewTab);
            
            HeaderText.text = OverviewTitle;
        }

        /// <summary>
        /// Shows the NewDeviceTab and hides the NewDeviceTab.
        /// Updates the header text to "Add new Device".
        /// </summary>
        private void ShowNewDeviceTab()
        {
            ShowTab(NewDeviceTab);
            
            HeaderText.text = NewDeviceTitle;
        }
        
        /// <summary>
        /// Shows the SettingsTab and hides the SettingsTab.
        /// Updates the header text to "Settings".
        /// </summary>
        internal void ShowSettingsTab()
        {
#if UNITY_VISIONOS && !UNITY_EDITOR
            SwiftUIDriver.OpenSwiftUIWindow("MainMenu", "Settings");
            HomeButtonSetActive(false);
            SoundManager.OnUIPressed();
            return;
#endif
            ShowTab(SettingsTab);
            
            HeaderText.text = SettingsTitle;
        }

        /// <summary>
        /// Shows the specified tab and hides all other tabs.
        /// Also makes sure the main UI is visible and the home button is hidden.
        /// </summary>
        /// <param name="tab">The tab to show.</param>
        private void ShowTab(GameObject tab)
        {            
            SoundManager.OnUIPressed();

            // Hide all other tabs
            OverviewTab.SetActive(false);
            SettingsTab.SetActive(false);
            NewDeviceTab.SetActive(false);
            
            // Show the selected tab
            tab.SetActive(true);
            
            // Show the main UI and hide the home button
            MainUI.SetActive(true);
            HomeButtonSetActive(false);
            
            // Disable lazy follow while the main UI is open
            if (_mainUILazyFollow == null)
                _mainUILazyFollow = MainUI.GetComponentInParent<LazyFollow>();
            if (_mainUILazyFollow != null)
                _mainUILazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.None;
        }

        private void HomeButtonSetActive(bool setActive)
        {
            if (!setActive && _canvasFader)
            {
                _canvasFader.FadeOut(true);
                return;
            }
            HomeButtonUI.SetActive(setActive);
        }

        /// <summary>
        /// Hides the main UI and shows the home button.
        /// Also activates the position follow mode of the LazyFollow.
        /// </summary>
        private void HideMainMenu()
        {
            if (!MainUI)
            {
                HomeButtonSetActive(true);
                return;
            }
            
            MainUI.SetActive(false);
            HomeButtonSetActive(true);
            
            if (_mainUILazyFollow == null)
                _mainUILazyFollow = MainUI.GetComponentInParent<LazyFollow>();
            if (_mainUILazyFollow != null)
                _mainUILazyFollow.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
        }

        public void CloseMainMenu()
        {
            HideMainMenu();
        }

        public void ShowHomeButton()
        {
            HomeButtonSetActive(true);
        }
    }
}