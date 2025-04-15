using System;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Panels
{
    public class PanelButton : Panel
    {
        [SerializeField] private Button Button;
        private IncrementalSlider _incrementalSlider;
        // Current brightness of the Light, stored to avoid overrides by Rest responses
        private int _brightnessValue;
    
        protected override void OnEnable()
        {
            base.OnEnable();
        
            Button.onClick.AddListener(OnButtonClicked);
            
            // Get the IncrementalSlider component attached to the button and subscribe to its event
            _incrementalSlider = Button.GetComponent<IncrementalSlider>();
            if (_incrementalSlider)
                _incrementalSlider.OnSliderValueChanged += OnIncrementalSliderValueChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            
            if (_incrementalSlider)
                _incrementalSlider.OnSliderValueChanged -= OnIncrementalSliderValueChanged;
        }

        /// <summary>
        /// Updates the panel's UI elements based on the current state.
        /// </summary>
        protected override void UpdatePanel()
        {
            base.UpdatePanel();
            
            if (!PanelIsReady())
                return;
            
            // Update the state text based on brightness or state
            if (HassState.attributes.brightness != 0)
                StateText.text = Mathf.Round((float)HassState.attributes.brightness / 255 * 100) + "%";
            else
                StateText.text = HassState.state;

            // Update the panel layout and icon
            WindowBehaviour.UpdatePanelLayout();
            UpdateIcon();
        }

        /// <summary>
        /// Toggles the entity associated with the entityID.
        /// </summary>
        private void OnButtonClicked()
        {
            if (!PanelIsReady())
                return;
        
            // Toggle the device based on its type
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
        /// Updates the icon color based on the panel's state and attributes.
        /// </summary>
        private void UpdateIcon()
        {
            if (!PanelIsReady())
                return;
        
            // Update the icon based on the panel's attributes.
            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            Color color;
        
            // Determine the icon color based on the panel's state
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
        
            Icon.color = color;
        }
    }
}