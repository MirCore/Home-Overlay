using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;

namespace Panel
{
    public class PanelSensor : Panels.Panel
    {
        private void UpdateSensorValue()
        {
            if (!PanelIsReady())
                return;
            
            // Sensor as a state string
            string stateText = HassState.state;
            // Sensor as a state float
            if (float.TryParse(HassState.state, NumberStyles.Any, CultureInfo.InvariantCulture, out float stateNumber))
                stateText = stateNumber.ToString("G3");
            // Sensor as a temperature float
            if (HassState.DeviceType == EDeviceType.CLIMATE)
                stateText = $"{HassState.attributes.current_temperature} ({HassState.attributes.temperature})"; // current (target)
            
            string sensorText = "";
            if (HassState.DeviceType == EDeviceType.CLIMATE && !string.IsNullOrEmpty(GameManager.Instance.HassConfig.unit_system.temperature))
                sensorText = $" {GameManager.Instance.HassConfig.unit_system.temperature}";
            else if (HassState.attributes != null)
                sensorText = $" {HassState.attributes.unit_of_measurement}";
            
            StateText.text = $"{stateText}{sensorText}";
        }

       
        protected override void UpdatePanel()
        {
            base.UpdatePanel();
            
            UpdatePanelLayout();
            UpdateSensorValue();
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            if (!PanelIsReady())
                return;
        
            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            if (HassState.DeviceType == EDeviceType.CLIMATE)
            {
                Icon.color = HassState.state == "heat" ? Icon.color = new Color(1f, 0.4f, 0f) : Icon.color = Color.black;
            }
            else
            {
                Icon.color = Color.white;
            }
        }
    }
}
