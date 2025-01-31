using System.Globalization;
using Managers;
using TMPro;
using UnityEngine;

namespace Entity
{
    public class EntitySensor : Entity
    {
        [SerializeField] private TMP_Text SensorState;
        [SerializeField] private TMP_Text SensorUnit;
    
        private void UpdateSensorValue()
        {
            if (EntityState == null)
                return;
            // Sensor as a state string
            string stateText = EntityState.state;
            // Sensor as a state float
            if (float.TryParse(EntityState.state, NumberStyles.Any, CultureInfo.InvariantCulture, out float stateNumber))
                stateText = stateNumber.ToString("G3");
            // Sensor as a temperature float
            if (EntityState.DeviceType == EDeviceType.CLIMATE)
                stateText = $"{EntityState.attributes.current_temperature} ({EntityState.attributes.temperature})"; // current (target)
            SensorState.text = $"{stateText}";
            SensorUnit.text = $"{(EntityState.attributes == null ? "" : EntityState.attributes.unit_of_measurement)}";
            if (EntityState.DeviceType == EDeviceType.CLIMATE && !string.IsNullOrEmpty(GameManager.Instance.HassConfig.unit_system.temperature))
                SensorUnit.text = $"{GameManager.Instance.HassConfig.unit_system.temperature}";
        }

        protected override void UpdateEntity()
        {
            UpdateSensorValue();
            UpdateIcon();
        }

        private void UpdateIcon()
        {
            if (EntityState == null)
                return;
        
            Icon.text = MaterialDesignIcons.GetIcon(EntityState);

            if (EntityState.DeviceType == EDeviceType.CLIMATE)
            {
                Icon.color = EntityState.state == "heat" ? Icon.color = new Color(1f, 0.4f, 0f) : Icon.color = Color.black;
            }
            else
            {
                Icon.color = Color.white;
            }
        }
    }
}
