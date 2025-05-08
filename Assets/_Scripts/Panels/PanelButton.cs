using System;
using Managers;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Panels
{
    /// <summary>
    /// A panel that represents a switch or a light.
    /// </summary>
    public class PanelButton : Panel
    {
        /// <summary>
        /// The button component of the panel.
        /// </summary>
        [SerializeField] private Button Button;

        /// <summary>
        /// The incremental slider component attached to the button.
        /// </summary>
        private IncrementalSlider _incrementalSlider;

        /// <summary>
        /// The current brightness value of the light, stored to avoid overrides by REST responses.
        /// </summary>
        private int _brightnessValue;

        protected override void OnEnable()
        {
            base.OnEnable();

            // Subscribe to the button-click event
            Button.onClick.AddListener(OnButtonClicked);

            // Get the IncrementalSlider component attached to the button and subscribe to its event
            _incrementalSlider = Button.GetComponent<IncrementalSlider>();
            if (_incrementalSlider)
                _incrementalSlider.OnSliderValueChanged += OnIncrementalSliderValueChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            // Unsubscribe from the incremental slider event
            if (_incrementalSlider)
                _incrementalSlider.OnSliderValueChanged -= OnIncrementalSliderValueChanged;
        }

        /// <summary>
        /// Updates the panel's UI elements based on the current state.
        /// </summary>
        protected override void UpdatePanel()
        {
            base.UpdatePanel();

            if (PanelData.IsDemoPanel)
                LoadDemoText();
            
            if (!PanelIsReady())
                return;

            // Update the state text based on brightness or state
            if (HassState.attributes.brightness != 0)
                StateText.text = Mathf.Round((float)HassState.attributes.brightness / 255 * 100) + "%";
            else
                StateText.text = HassState.state;

            // Update the panel icon
            UpdateIcon();
        }

        /// <summary>
        /// Loads and updates the demo text for the panel's name, state, and icon.
        /// </summary>
        /// <param name="on">Determines whether the state text reflects the panel as active ("on") or inactive ("off").</param>
        private void LoadDemoText(bool on = true)
        {
            NameText.text = "Demo Light";
            StateText.text = on ? "90%" : "off";
            Icon.text = MaterialDesignIcons.GetIcon("lightbulb");
            Icon.color = StateText.text == "off" ? Color.black : Color.white;
        }

        /// <summary>
        /// Toggles the entity associated with the entityID.
        /// </summary>
        private void OnButtonClicked()
        {
            SoundManager.OnUIPressed();
            
            if (PanelData.IsDemoPanel)
                LoadDemoText(StateText.text == "off");
            if (!PanelIsReady())
                return;

            // Send Toggle command based on the device type
            switch (HassState.DeviceType)
            {
                case EDeviceType.LIGHT:
                    RestHandler.ToggleLight(PanelData.EntityID);
                    break;
                case EDeviceType.SWITCH:
                    RestHandler.ToggleSwitch(PanelData.EntityID);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Handles the event when the incremental slider value changes.
        /// </summary>
        /// <param name="brightnessDelta">The change in brightness value.</param>
        /// <param name="firstDrag">Indicates if this is the first drag event.</param>
        private void OnIncrementalSliderValueChanged(float brightnessDelta, bool firstDrag)
        {
            if (!PanelIsReady() || HassState.DeviceType != EDeviceType.LIGHT)
                return;

            // Set the initial brightness value on the first drag
            if (firstDrag)
                _brightnessValue = HassStates.GetHassState(PanelData.EntityID).attributes.brightness;

            // Update the brightness value within the valid range
            _brightnessValue = Math.Clamp(_brightnessValue + (int)brightnessDelta, 0, 255);
            RestHandler.SetLightBrightness(PanelData.EntityID, _brightnessValue);
        }

        /// <summary>
        /// Updates the icon color based on the entities state
        /// </summary>
        private void UpdateIcon()
        {
            if (!PanelIsReady() || PanelData.IsDemoPanel)
                return;

            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            // Set the icon color based on the entity state and brightness
            Icon.color = DetermineIconColor();
        }

        /// <summary>
        /// Determines the icon color based on the entity state and brightness.
        /// </summary>
        /// <returns>The color to set for the icon.</returns>
        private Color DetermineIconColor()
        {
            Color color;

            // Determine the icon color based on the entities state
            if (HassState.state == "off")
            {
                color = Color.black;
            }
            else if (HassState.attributes is { rgb_color: { Length: 3 } })
            {
                color = JsonHelpers.RGBToUnityColor(HassState.attributes.rgb_color);
            }
            else
            {
                color = Color.white;
            }

            // Adjust the color based on brightness
            if (HassState.attributes != null && HassState.attributes.brightness != 0)
            {
                color = Color.Lerp(Color.black, color, HassState.attributes.brightness / 255f);
            }

            return color;
        }
    }
}
