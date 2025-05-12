using Managers;
using Structs;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Panels
{
    /// <summary>
    /// Abstract base class for panels in the environment, handling initialization, updates, and settings.
    /// </summary>
    public abstract class Panel : Window
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
        /// The compact size of the panel's canvas.
        /// </summary>
        [SerializeField] public Vector2 CompactCanvasSize = new(120, 120);

        /// <summary>
        /// The expanded size of the panel's canvas.
        /// </summary>
        [SerializeField] public Vector2 ExpandedCanvasSize = new(320, 120);

        /// <summary>
        /// The component responsible for highlighting the window.
        /// </summary>
        public WindowHighlighter WindowHighlighter;
        
        /// <summary>
        /// The current state of the Home Assistant entity associated with the panel.
        /// </summary>
        protected HassState HassState;

        /// <summary>
        /// The data associated with this panel.
        /// </summary>
        public PanelData PanelData { get; private set; }

        protected new virtual void OnEnable()
        {
            base.OnEnable();
            
            // Get the MeshRenderer component from the window
            Renderer meshRenderer = GetComponentInChildren<MeshRenderer>();
            WindowHighlighter = new WindowHighlighter(meshRenderer);

            // Subscribe to the settings button click event
            SettingsButton.onClick.AddListener(OnSettingsButtonClicked);
            // Subscribe to the Home Assistant states changed event
            EventManager.OnHassStatesChanged += UpdatePanel;
        }

        protected new virtual void OnDisable()
        {
            base.OnDisable();
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

            // Initialize panel position - either create a new anchor or attach to an existing anchor
            if (string.IsNullOrEmpty(PanelData.AnchorID))
                SetNewPanelPose();
            else
                AnchorHelper.TryAttachToExistingAnchor(transform, !PanelData.Settings._rotationEnabled, PanelData.AnchorID);
            // Load the window state based on the panel settings
            LoadWindowState(PanelData.Settings.RotationEnabled, !PanelData.Settings.HideWindowControls);

            // Update the panel to reflect the current state of the entity
            UpdatePanel();
        }

        /// <summary>
        /// Updates the panel to reflect the current state of the Home Assistant entity.
        /// </summary>
        protected virtual void UpdatePanel()
        {
            // Update the panel layout
            UpdatePanelLayout();
            
            if (!PanelIsReady())
                return;
            
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
        /// Handles changes to the panel's settings.
        /// </summary>
        public void OnSettingsChanged()
        {
            // Load the window state based on the updated panel settings
            LoadWindowState(PanelData.Settings.RotationEnabled, !PanelData.Settings.HideWindowControls);

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
        
        /// <inheritdoc />
        public override void OnDragStarted()
        {
            base.OnDragStarted();
            PanelSettingsWindowManager.Instance.PanelIsMoving(this, true);
        }
        
        /// <inheritdoc />
        public override void OnDragEnded()
        {
            base.OnDragEnded();
            PanelSettingsWindowManager.Instance.PanelIsMoving(this, false);

            SetNewPanelPose();
        }
        
        /// <inheritdoc />
        protected override void UpdateWindowSize(bool expanded, bool compact)
        {
            CanvasRectTransform.sizeDelta = expanded || compact ? ExpandedCanvasSize : CompactCanvasSize;
            base.UpdateWindowSize(expanded, compact);
        }
        
        /// <summary>
        /// Handles the event when the panel is scaled.
        /// </summary>
        public void OnScaled()
        {
            PanelData.Scale = transform.localScale;
        }

        /// <summary>
        /// Sets the Pose to PanelData and creates a new Anchor
        /// </summary>
        private void SetNewPanelPose()
        {
            if (PanelData == null)
                return;

            // Save the new pose of the panel
            PanelData.Position = transform.position;
            PanelData.Rotation = transform.rotation;
            PanelData.Scale = transform.localScale;

            // Create a new anchor
            AnchorHelper.CreateNewAnchor(this);
        }

        /// <summary>
        /// Updates the panel layout based on the panel's settings.
        /// </summary>
        protected void UpdatePanelLayout()
        {
            if (PanelData.EntityID == null) return;
            
            bool showName = PanelData.Settings.ShowName;
            bool showState = PanelData.Settings.ShowState;

            if ((!NameText || NameText.gameObject.activeSelf == showName) &&
                (!StateText || StateText.gameObject.activeSelf == showState))
                return;

            NameText?.gameObject.SetActive(showName);
            StateText?.gameObject.SetActive(showState);

            UpdateWindowSize(showName, showState);
        }

        /// <summary>
        /// Aligns the panel to the nearest wall by creating a new anchor for the panel.
        /// </summary>
        public void AlignPanelToWall()
        {
            AnchorHelper.CreateNewAnchor(this, true);
            PanelData.Position = transform.position;
            PanelData.Rotation = transform.rotation;
        }

        /// <summary>
        /// Invoked when the panel fails to align to a wall.
        /// </summary>
        public void OnAlignToWallFailed()
        {
            PanelSettingsWindowManager.Instance.OnAlignToWallFailed(this);
        }
    }
}