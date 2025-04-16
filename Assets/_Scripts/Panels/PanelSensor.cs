using System.Globalization;
using UnityEngine;

namespace Panels
{
    /// <summary>
    /// A panel that displays sensor data from Home Assistant.
    /// </summary>
    public class PanelSensor : Panel
    {
        /// <summary>
        /// Updates the sensor value displayed on the panel.
        /// </summary>
        private void UpdateSensorValue()
        {
            if (!PanelIsReady())
                return;

            // Get the state text and sensor text
            string stateText = GetStateText();
            string sensorText = GetSensorText();

            // Update the state text with the sensor value
            StateText.text = $"{stateText}{sensorText}";
        }

        /// <summary>
        /// Gets the state text based on the sensor type and state.
        /// </summary>
        /// <returns>The state text to display.</returns>
        private string GetStateText()
        {
            // Default state text
            string stateText = HassState.state;

            // If the state is a float, format it as a general number
            if (float.TryParse(HassState.state, NumberStyles.Any, CultureInfo.InvariantCulture, out float stateNumber))
                stateText = stateNumber.ToString("G3");

            // If the device type is climate, display the current and target temperature
            if (HassState.DeviceType == EDeviceType.CLIMATE)
                stateText = $"{HassState.attributes.current_temperature} ({HassState.attributes.temperature})"; // current (target)

            return stateText;
        }

        /// <summary>
        /// Gets the sensor text based on the sensor type and attributes.
        /// </summary>
        /// <returns>The sensor text to display.</returns>
        private string GetSensorText()
        {
            string sensorText = "";

            // If the device type is climate and the temperature unit is not empty, append the temperature unit
            if (HassState.DeviceType == EDeviceType.CLIMATE && !string.IsNullOrEmpty(HassStates.GetHassConfig().unit_system.temperature))
                sensorText = $" {HassStates.GetHassConfig().unit_system.temperature}";
            else if (HassState.attributes != null)
                sensorText = $" {HassState.attributes.unit_of_measurement}";

            return sensorText;
        }

        /// <summary>
        /// Updates the panels UI elements based on the current state.
        /// </summary>
        protected override void UpdatePanel()
        {
            base.UpdatePanel();

            // Update the panel layout and sensor value
            WindowBehaviour.UpdatePanelLayout();
            UpdateSensorValue();
            UpdateIcon();
        }

        /// <summary>
        /// Updates the icon color based on the entities state and attributes.
        /// </summary>
        private void UpdateIcon()
        {
            if (!PanelIsReady())
                return;

            // Update the icon based on the entities state
            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            // Set the icon color based on the entities state
            Icon.color = DetermineIconColor();
        }

        /// <summary>
        /// Determines the icon color based on the entities state
        /// </summary>
        /// <returns>The color to set for the icon.</returns>
        private Color DetermineIconColor()
        {
            // If the device type is climate, set the icon color based on the state
            if (HassState.DeviceType == EDeviceType.CLIMATE)
            {
                return HassState.state == "heat" ? new Color(1f, 0.4f, 0f) : Color.black;
            }

            // Default icon color
            return Color.white;
        }
    }
}
