using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

namespace UI
{
    public class EntitySettingsUI : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TMP_Text TitleText;
        [SerializeField] private TMP_Text SubtitleText;
        
        [Header("Color Picker")]
        [SerializeField] private ColorPicker ColorPicker;
        
        [Header("Entity Settings")]
        [SerializeField] private Toggle WindowControlToggle;
        [SerializeField] private Toggle AlignWindowToWallToggle;
        [SerializeField] private Toggle RotationToggle;
        [SerializeField] private GameObject TogglesPanel;
        
        [SerializeField] private Button ChangeEntityButton;
        [SerializeField] private EntityPicker EntityPicker;
        
        /// <summary>
        /// The Button that deletes the Entity.
        /// </summary>
        [SerializeField] private Button DeleteButton;
        
        /// <summary>
        /// The currently selected device type.
        /// </summary>
        private EDeviceType _selectedEDeviceType;
        
        private HassState _hassState;

        public Entity.Entity Entity { get; private set; }


        private void OnEnable()
        {
            if (Entity != null)
                GetHassEntities();
            EntityPicker.gameObject.SetActive(false);
            UpdateHeader();
            LoadElements();
            
            ChangeEntityButton.gameObject.SetActive(true);
            TogglesPanel.SetActive(true);
            
            ChangeEntityButton.onClick.AddListener(OnChangeEntityButtonClicked);
            DeleteButton.onClick.AddListener(OnDeleteButtonClicked);
            WindowControlToggle.onValueChanged.AddListener(OnWindowControlToggleValueChanged);
            AlignWindowToWallToggle.onValueChanged.AddListener(OnAlignToWallToggleValueChanged);
            RotationToggle.onValueChanged.AddListener(OnRotationToggleValueChanged);
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
        }

        private void OnDisable()
        {
            ChangeEntityButton.onClick.RemoveListener(OnChangeEntityButtonClicked);
            DeleteButton.onClick.RemoveListener(OnDeleteButtonClicked);
            WindowControlToggle.onValueChanged.RemoveListener(OnWindowControlToggleValueChanged);
            AlignWindowToWallToggle.onValueChanged.RemoveListener(OnAlignToWallToggleValueChanged);
            RotationToggle.onValueChanged.RemoveListener(OnRotationToggleValueChanged);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }
        
        private void OnWindowControlToggleValueChanged(bool value)
        {
            Entity.EntityObject.Settings.HideWindowControls = value;
            Entity.SetWindowControlVisibility();
        }
        
        private void OnAlignToWallToggleValueChanged(bool value)
        {
            Entity.EntityObject.Settings.AlignWindowToWall = value;
            if (value == true)
                Entity.EntityObject.Settings.RotationEnabled = false;
            Entity.UpdateRotationBehaviour();
        }
        
        private void OnRotationToggleValueChanged(bool value)
        {
            Entity.EntityObject.Settings.RotationEnabled = value;
            if (value == true)
                Entity.EntityObject.Settings.AlignWindowToWall = false;
            Entity.UpdateRotationBehaviour();
        }

        /// <summary>
        /// Changes the UI to show the EntityPicker.
        /// </summary>
        private void OnChangeEntityButtonClicked()
        {
            // Deactivate other UI elements. The Settings UI will be recreated later, to restore the state.
            ChangeEntityButton.gameObject.SetActive(false);
            ColorPicker.gameObject.SetActive(false);
            TogglesPanel.SetActive(false);
            EntityPicker.gameObject.SetActive(true);
            
            EntityPicker.SetEntity(Entity);
        }
 
        private void OnDeleteButtonClicked()
        {
            Entity.DeleteEntity();
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
        /// Loads the entity state associated with the current entity.
        /// If the entity state is null, returns true.
        /// </summary>
        /// <returns>True if the entity state is null, false otherwise.</returns>
        private bool LoadEntityState()
        {
            if (Entity)
                _hassState = HassStates.GetHassState(Entity.EntityObject.EntityID);
            return _hassState == null;
        }

        public void SetEntity(Entity.Entity entity)
        {
            Entity = entity;
            ColorPicker.SetEntityID(Entity.EntityObject.EntityID);
            UpdateHeader();
            LoadElements();
        }

        private void LoadElements()
        {
            if (LoadEntityState()) 
                return;
            if (EntityPicker.gameObject.activeSelf)
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

            WindowControlToggle.SetIsOnWithoutNotify(Entity.EntityObject.Settings.HideWindowControls);
            AlignWindowToWallToggle.SetIsOnWithoutNotify(Entity.EntityObject.Settings.AlignWindowToWall);
            RotationToggle.SetIsOnWithoutNotify(Entity.EntityObject.Settings.RotationEnabled);
        }
    }
}
