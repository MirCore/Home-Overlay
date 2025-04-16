using System;
using System.Collections;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;
using Utils;

namespace UI
{
    public class PanelSettingsUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private Button CloseButton;
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text SubtitleText;
        
        [Header("Color Picker")]
        [SerializeField] private ColorPicker ColorPicker;
        
        [Header("Panel Settings")]
        [SerializeField] private Toggle ShowNameToggle;
        [SerializeField] private Toggle ShowStateToggle;
        [SerializeField] private Toggle WindowControlToggle;
        [SerializeField] private Toggle AlignWindowToWallToggle;
        [SerializeField] private Toggle RotationToggle;
        
        /// <summary>
        /// The Button that deletes the Panel.
        /// </summary>
        [SerializeField] private Button DeleteButton;
        
        [SerializeField] private DynamicScrollRect DynamicScrollRect;
        private WindowHighlighter _backgroundUtils;
        
        /// <summary>
        /// The currently selected device type.
        /// </summary>
        private EDeviceType _selectedEDeviceType;
        
        private HassState _hassState;

        public Panels.Panel Panel { get; private set; }

        private void Awake()
        {
            _backgroundUtils = new WindowHighlighter(GetComponent<MeshRenderer>());
        }

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
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        private void LookAtCamera()
        {
            if (Camera.main == null) return;
            
            LazyFollow lazyFollow = gameObject.GetComponent<LazyFollow>();
            lazyFollow.enabled = false;            
            Vector3 directionToCamera = transform.position - Camera.main.transform.position;
            transform.rotation = Quaternion.LookRotation(directionToCamera);
            lazyFollow.enabled = true;
        }

        private void OnDisable()
        {
            CloseButton.onClick.RemoveListener(OnCloseButtonClicked);
            DeleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            ShowNameToggle.onValueChanged.RemoveListener(OnShowNameChanged);
            ShowStateToggle.onValueChanged.RemoveListener(OnShowStateChanged);
            WindowControlToggle.onValueChanged.RemoveListener(OnWindowControlToggleValueChanged);
            AlignWindowToWallToggle.onValueChanged.RemoveListener(OnAlignToWallToggleValueChanged);
            RotationToggle.onValueChanged.RemoveListener(OnRotationToggleValueChanged);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }

        private void OnCloseButtonClicked()
        {
            GetComponent<CanvasFader>().FadeOut(true);
        }

        private void OnShowNameChanged(bool value)
        {
            Panel.PanelData.Settings.ShowName = value;
            Panel.OnSettingsChanged();
        }

        private void OnShowStateChanged(bool value)
        {
            Panel.PanelData.Settings.ShowState = value;
            Panel.OnSettingsChanged();
        }

        private void OnWindowControlToggleValueChanged(bool value)
        {
            Panel.PanelData.Settings.HideWindowControls = value;
            Panel.SetWindowControlVisibility(!Panel.PanelData.Settings.HideWindowControls);
        }
        
        private void OnAlignToWallToggleValueChanged(bool value)
        {
            Panel.PanelData.Settings.AlignWindowToWall = value;
            if (value == true)
                Panel.PanelData.Settings.RotationEnabled = false;
            Panel.OnSettingsChanged();
        }
        
        private void OnRotationToggleValueChanged(bool value)
        {
            Panel.PanelData.Settings.RotationEnabled = value;
            if (value == true)
                Panel.PanelData.Settings.AlignWindowToWall = false;
            Panel.OnSettingsChanged();
        }
 
        private void OnDeleteButtonClicked()
        {
            PanelManager.Instance.DeletePanel(Panel.PanelData.ID);
        }

        /// <summary>
        /// Updates the EntityDropdown when the HassStates are changed
        /// </summary>
        private void OnHassStatesChanged()
        {
            UpdateHeader();
            LoadElements();
        }

        private void UpdateHeader()
        {
            if (LoadEntityState()) return;

            TitleText.text = _hassState.attributes.friendly_name;
            SubtitleText.text = _hassState.entity_id;
        }

        /// <summary>
        /// Loads the panel state associated with the current panel.
        /// If the panel state is null, returns true.
        /// </summary>
        /// <returns>True if the panel state is null, false otherwise.</returns>
        private bool LoadEntityState()
        {
            if (Panel)
                _hassState = HassStates.GetHassState(Panel.PanelData.EntityID);
            return _hassState == null;
        }

        public void SetPanel(Panels.Panel panel)
        {
            Panel = panel;
            ColorPicker.SetEntityID(Panel.PanelData.EntityID);
            UpdateHeader();
            LoadElements();
            
            LookAtCamera();
        }

        private void LoadElements()
        {
            if (LoadEntityState()) 
                return;

            if (_hassState.DeviceType == EDeviceType.LIGHT && _hassState.attributes.supported_color_modes.Length != 0)
            {
                ColorPicker.gameObject.SetActive(true);
                ColorPicker.SetMode(_hassState.attributes.supported_color_modes);
            }
            else
            {
                ColorPicker.gameObject.SetActive(false);
            }

            ShowNameToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.ShowName);
            ShowStateToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.ShowState);
            WindowControlToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.HideWindowControls);
            AlignWindowToWallToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.AlignWindowToWall);
            RotationToggle.SetIsOnWithoutNotify(Panel.PanelData.Settings.RotationEnabled);
            
            DynamicScrollRect?.OnContentSizeChanged();
        }

        public void SetActive(bool setActive)
        {
            if (setActive)
                gameObject.SetActive(true);
            else
                OnCloseButtonClicked();
        }

        public void ReloadUI()
        {
            LoadElements();
        }

        public void IsMoving(bool isMoving)
        {
            GetComponent<CanvasFader>().FadeInOut(!isMoving);
        }
    }
}
