using System.Collections;
using JetXR.VisionUI;
using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace UI
{
    /// <summary>
    /// Manages the settings UI for a panel.
    /// </summary>
    public class PanelSettingsUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button CloseButton;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text SubtitleText;
        
        [Header("Color Picker")]
        [SerializeField] private ColorPicker ColorPicker;
        
        /// <summary>
        /// Toggle for showing/hiding the panel name.
        /// </summary>
        [Header("Panel Settings")]
        [SerializeField] private Toggle ShowNameToggle;
        
        /// <summary>
        /// Toggle for showing/hiding the panel state.
        /// </summary>
        [SerializeField] private Toggle ShowStateToggle;
        
        /// <summary>
        /// Toggle for enabling/disabling panels' window controls.
        /// </summary>
        [SerializeField] private Toggle WindowControlToggle;
        
        /// <summary>
        /// Toggle for aligning the window to the wall.
        /// </summary>
        [SerializeField] private Toggle AlignWindowToWallToggle;
        
        /// <summary>
        /// Toggle for enabling/disabling rotation.
        /// </summary>
        [SerializeField] private Toggle RotationToggle;

        /// <summary>
        /// Label for the AlignToWall toggle
        /// </summary>
        [SerializeField] private TMP_Text AlignToWallText;
        
        /// <summary>
        /// Button that deletes the Panel.
        /// </summary>
        [SerializeField] private Button DeleteButton;
        
        /// <summary>
        /// ScrollRect component for dynamic content sizing of the settings UI.
        /// </summary>
        [SerializeField] private DynamicScrollRect DynamicScrollRect;
        
        /// <summary>
        /// The currently selected device type.
        /// </summary>
        private EDeviceType _selectedEDeviceType;
        
        /// <summary>
        /// Animation component for the show name toggle.
        /// </summary>
        private ToggleAnimation _showNameToggleAnimation;
        
        /// <summary>
        /// Animation component for the show state toggle.
        /// </summary>
        private ToggleAnimation _showStateToggleAnimation;
        
        /// <summary>
        /// Animation component for the window control toggle.
        /// </summary>
        private ToggleAnimation _windowControlToggleAnimation;
        
        /// <summary>
        /// Animation component for the align-window-to-wall toggle.
        /// </summary>
        private ToggleAnimation _alignWindowToWallToggleAnimation;
        
        /// <summary>
        /// Animation component for the rotation toggle.
        /// </summary>
        private ToggleAnimation _rotationToggleAnimation;

        /// <summary>
        /// Indicates whether the panel is currently attempting to align itself to a nearby wall.
        /// </summary>
        private bool _tryingToAttachToWall;

        /// <summary>
        /// The panel associated with this settingsUI.
        /// </summary>
        public Panels.Panel Panel { get; private set; }

        /// <summary>
        /// Initializes toggle animation components.
        /// </summary>
        private void Awake()
        {
            _rotationToggleAnimation = RotationToggle.GetComponent<ToggleAnimation>();
            _alignWindowToWallToggleAnimation = AlignWindowToWallToggle.GetComponent<ToggleAnimation>();
            _windowControlToggleAnimation = WindowControlToggle.GetComponent<ToggleAnimation>();
            _showStateToggleAnimation = ShowStateToggle.GetComponent<ToggleAnimation>();
            _showNameToggleAnimation = ShowNameToggle.GetComponent<ToggleAnimation>();
        }

        /// <summary>
        /// Sets up event listeners and initializes UI elements when enabled.
        /// </summary>
        private void OnEnable()
        {
            UpdateHeader();
            LoadElements();
            
            LookAtCamera();
            
            CloseButton.onClick.AddListener(OnCloseButtonClicked);
            DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
            ShowNameToggle.onValueChanged.AddListener(OnShowNameChanged);
            ShowStateToggle.onValueChanged.AddListener(OnShowStateChanged);
            WindowControlToggle.onValueChanged.AddListener(OnWindowControlToggleValueChanged);
            AlignWindowToWallToggle.onValueChanged.AddListener(OnAlignToWallToggleValueChanged);
            RotationToggle.onValueChanged.AddListener(OnRotationToggleValueChanged);
        }

        /// <summary>
        /// Makes the panel face the main camera. To avoid LazyFollow slowly rotating towards the user.
        /// </summary>
        private void LookAtCamera()
        {
            if (Camera.main == null) return;
            
            LazyFollow lazyFollow = gameObject.GetComponent<LazyFollow>();
            lazyFollow.enabled = false;            
            Vector3 directionToCamera = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
            lazyFollow.enabled = true;
        }

        /// <summary>
        /// Removes event listeners when disabled.
        /// </summary>
        private void OnDisable()
        {
            CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
            DeleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            ShowNameToggle.onValueChanged.RemoveListener(OnShowNameChanged);
            ShowStateToggle.onValueChanged.RemoveListener(OnShowStateChanged);
            WindowControlToggle.onValueChanged.RemoveListener(OnWindowControlToggleValueChanged);
            AlignWindowToWallToggle.onValueChanged.RemoveListener(OnAlignToWallToggleValueChanged);
            RotationToggle.onValueChanged.RemoveListener(OnRotationToggleValueChanged);
        }

        /// <summary>
        /// Handles the close button click event.
        /// </summary>
        private void OnCloseButtonClicked()
        {
            SoundManager.OnUIPressed();
            GetComponent<CanvasFader>().FadeOut(true);
        }

        /// <summary>
        /// Handles changes to the show name toggle.
        /// </summary>
        /// <param name="value">New toggle value</param>
        private void OnShowNameChanged(bool value)
        {
            SoundManager.OnUIPressed();
            Panel.PanelData.Settings.ShowName = value;
            Panel.OnSettingsChanged();
        }

        /// <summary>
        /// Handles changes to the show state toggle.
        /// </summary>
        /// <param name="value">New toggle value</param>
        private void OnShowStateChanged(bool value)
        {
            SoundManager.OnUIPressed();
            Panel.PanelData.Settings.ShowState = value;
            Panel.OnSettingsChanged();
        }

        /// <summary>
        /// Handles changes to the window control toggle.
        /// </summary>
        /// <param name="value">New toggle value</param>
        private void OnWindowControlToggleValueChanged(bool value)
        {
            SoundManager.OnUIPressed();
            Panel.PanelData.Settings.HideWindowControls = value;
            Panel.SetWindowControlVisibility(!Panel.PanelData.Settings.HideWindowControls);
        }
        
        /// <summary>
        /// Handles changes to the align-to-wall toggle.
        /// </summary>
        /// <param name="value">New toggle value</param>
        private void OnAlignToWallToggleValueChanged(bool value)
        {
            SoundManager.OnUIPressed();
            Panel.PanelData.Settings.AlignWindowToWall = value;
            _tryingToAttachToWall = value;
            if (value)
            {
                Panel.PanelData.Settings.RotationEnabled = false;
                Panel.AlignPanelToWall();
            }
            Panel.OnSettingsChanged();
        }
        
        /// <summary>
        /// Handles changes to the rotation toggle.
        /// </summary>
        /// <param name="value">New toggle value</param>
        private void OnRotationToggleValueChanged(bool value)
        {
            SoundManager.OnUIPressed();
            Panel.PanelData.Settings.RotationEnabled = !value;
            if (value == false)
                Panel.PanelData.Settings.AlignWindowToWall = false;
            Panel.OnSettingsChanged();
        }
 
        /// <summary>
        /// Handles the delete button click event.
        /// </summary>
        private void OnDeleteButtonClicked()
        {
            SoundManager.OnUIPressed();
            PanelManager.Instance.DeletePanel(Panel.PanelData.ID);
        }

        /// <summary>
        /// Updates the header text elements based on the current panel's data.
        /// </summary>
        private void UpdateHeader()
        {
            if (!Panel)
                return;
            
            if (Panel.PanelData.IsDemoPanel)
            {
                TitleText.text = "Demo " + Panel.PanelData.EntityID.Split('.')[0];
                SubtitleText.text = Panel.PanelData.EntityID;
                return;
            }

            HassState hassState = HassStates.GetHassState(Panel.PanelData.EntityID);
            if (hassState != null)
                TitleText.text = hassState.attributes.friendly_name;
            SubtitleText.text = Panel.PanelData.EntityID;
        }

        /// <summary>
        /// Sets the panel associated with this settings UI.
        /// </summary>
        /// <param name="panel">Panel to associate with the settings UI</param>
        public void SetPanel(Panels.Panel panel)
        {
            Panel = panel;
            ColorPicker.SetPanelData(Panel.PanelData);
            UpdateHeader();
            LoadElements();
            
            LookAtCamera();
        }

        /// <summary>
        /// Loads and updates UI elements based on panel settings.
        /// </summary>
        private void LoadElements()
        {
            if (!Panel)
                return;
            
            EDeviceType deviceType = HassStates.GetDeviceType(Panel.PanelData.EntityID);
            
            if (deviceType is EDeviceType.WEATHER or EDeviceType.CALENDAR)
            {
                ShowNameToggle.transform.parent.gameObject.SetActive(false);
                ShowStateToggle.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                ShowNameToggle.transform.parent.gameObject.SetActive(true);
                ShowStateToggle.transform.parent.gameObject.SetActive(true);
                ShowNameToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.ShowName);
                _showNameToggleAnimation.Changed();
                ShowStateToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.ShowState);
                _showStateToggleAnimation.Changed();
            }

            WindowControlToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.HideWindowControls);
            _windowControlToggleAnimation.Changed();
            AlignWindowToWallToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.AlignWindowToWall);
            _alignWindowToWallToggleAnimation.Changed();
            RotationToggle.SetIsOnWithoutNotify(!Panel.PanelData.Settings.RotationEnabled);
            _rotationToggleAnimation.Changed();
            
            DynamicScrollRect?.OnContentSizeChanged();
        }
        
        /// <summary>
        /// Reloads the UI after a one-frame delay, to prevent Toggles visuals getting stuck.
        /// </summary>
        /// <returns>Coroutine IEnumerator</returns>
        public IEnumerator DelayedReloadUI()
        {
            yield return null;  // Wait for one frame
            LoadElements();

            // Show an alert that AlignToWall failed
            if (!_tryingToAttachToWall || AlignWindowToWallToggle.isOn)
                yield break;
            _tryingToAttachToWall = false;
            string originalText = AlignToWallText.text;
            AlignToWallText.text = "No wall detected. Locking rotation instead.";
            AlignToWallText.fontStyle = FontStyles.Italic;
            yield return new WaitForSeconds(3);
            AlignToWallText.text = originalText;
            AlignToWallText.fontStyle = FontStyles.Normal;
        }

        /// <summary>
        /// Sets the active state of the settings UI.
        /// </summary>
        /// <param name="setActive">Whether to set the UI active or inactive</param>
        public void SetActive(bool setActive)
        {
            if (setActive)
                gameObject.SetActive(true);
            else
                OnCloseButtonClicked();
        }

        /// <summary>
        /// Handles panel movement state changes.
        /// </summary>
        /// <param name="isMoving">Whether the panel is currently moving</param>
        public void IsMoving(bool isMoving)
        {
            GetComponent<CanvasFader>().FadeInOut(!isMoving);
        }
    }
}