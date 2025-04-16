using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Panels
{
    /// <summary>
    /// Abstract base class for panels in the UI, handling initialization, updates, and settings.
    /// </summary>
    public abstract class Panel : MonoBehaviour
    {
        /// <summary>
        /// The icon text component of the panel.
        /// </summary>
        [SerializeField] internal TMP_Text Icon;

        /// <summary>
        /// The name text component of the panel.
        /// </summary>
        [SerializeField] internal TMP_Text NameText;

        /// <summary>
        /// The state text component of the panel.
        /// </summary>
        [SerializeField] internal TMP_Text StateText;

        /// <summary>
        /// The settings button of the panel.
        /// </summary>
        [SerializeField] private Button SettingsButton;

        /// <summary>
        /// The window controls game object of the panel.
        /// </summary>
        [SerializeField] private GameObject WindowControls;

        /// <summary>
        /// The compact size of the panel's canvas.
        /// </summary>
        [SerializeField] public Vector2 CompactCanvasSize = new(120, 120);

        /// <summary>
        /// The expanded size of the panel's canvas.
        /// </summary>
        [SerializeField] public Vector2 ExpandedCanvasSize = new(320, 120);

        
        /// <summary>
        /// The current state of the Home Assistant entity associated with the panel.
        /// </summary>
        protected HassState HassState;

        /// <summary>
        /// The data associated with this panel.
        /// </summary>
        public PanelData PanelData { get; private set; }

        /// <summary>
        /// The behavior controlling the window's state and interactions.
        /// </summary>
        public WindowBehaviour WindowBehaviour;

        /// <summary>
        /// Initializes the window behavior when the panel is awakened.
        /// </summary>
        private void Awake()
        {
            WindowBehaviour = new WindowBehaviour(this, WindowControls);
        }

        protected virtual void OnEnable()
        {
            // Clear the name and state text
            if (NameText) NameText.text = "";
            if (StateText) StateText.text = "";

            // Subscribe to the settings button click event
            SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
            // Register event listeners for window behavior
            WindowBehaviour.RegisterEventListeners();
            // Subscribe to the Home Assistant states changed event
            EventManager.OnHassStatesChanged += UpdatePanel;
        }

        protected virtual void OnDisable()
        {
            // Unregister event listeners for window behavior
            WindowBehaviour.UnregisterEventListeners();
            // Unsubscribe from the settings button click event
            SettingsButton.onClick.RemoveListener(OnSettingsButtonClicked);
            // Unsubscribe from the Home Assistant states changed event
            EventManager.OnHassStatesChanged -= UpdatePanel;
        }

        /// <summary>
        /// Initializes the panel with the provided panel data.
        /// </summary>
        /// <param name="panelData">The panel data to associate with this panel.</param>
        public void InitPanel(PanelData panelData)
        {
            // Assign the provided panel data to the current panel
            PanelData = panelData;
            transform.localPosition = panelData.Position;
            transform.localScale = panelData.Scale;
            panelData.Panel = this;

            // Try to attach to the panels anchor
            AnchorHelper.TryAttachToExistingAnchor(transform, PanelData.AnchorID);
            // Load the window state based on the panel settings
            WindowBehaviour.LoadWindowState(PanelData.Settings.AlignWindowToWall, PanelData.Settings.RotationEnabled, !PanelData.Settings.HideWindowControls);

            // Update the panel to reflect the current state of the entity
            UpdatePanel();
        }

        /// <summary>
        /// Updates the panel to reflect the current state of the Home Assistant entity.
        /// </summary>
        protected virtual void UpdatePanel()
        {
            if (!PanelIsReady())
                return;

            // Update the panel layout
            WindowBehaviour.UpdatePanelLayout();
            
            // Update the name text with the friendly name of the entity
            if (NameText) NameText.text = HassState.attributes.friendly_name;
        }

        /// <summary>
        /// Handles the settings button click event.
        /// </summary>
        private void OnSettingsButtonClicked()
        {
            PanelSettingsWindowManager.Instance.ToggleSettingsWindow(this);
        }

        /// <summary>
        /// Turns off the align window to wall setting and updates the panel.
        /// </summary>
        public void TurnOffAlignWindowToWall()
        {
            Debug.Log("AlignWindowToWall failed, turning off AlignWindowToWall");
            PanelData.Settings.AlignWindowToWall = false;
            OnSettingsChanged();
        }

        /// <summary>
        /// Handles changes to the panel's settings.
        /// </summary>
        public void OnSettingsChanged()
        {
            // Load the window state based on the updated panel settings
            WindowBehaviour.LoadWindowState(PanelData.Settings.AlignWindowToWall, PanelData.Settings.RotationEnabled, !PanelData.Settings.HideWindowControls);

            // Update the panel to reflect the current state of the entity
            UpdatePanel();
            // Update the panel settings window
            PanelSettingsWindowManager.Instance.UpdatePanelSettingsWindow(this);
        }

        /// <summary>
        /// Checks if the panel is ready
        /// </summary>
        /// <returns>True if the panel is ready, false otherwise.</returns>
        protected bool PanelIsReady()
        {
            if (PanelData.EntityID == null) return false;

            // Get the current state of the Home Assistant entity
            HassState ??= HassStates.GetHassState(PanelData.EntityID);

            return HassState != null;
        }
    }
}
