using System.Globalization;
using Managers;
using Structs;
using TMPro;
using UnityEngine;

public class EntitySensor : Entity
{
    [SerializeField] private TMP_Text SensorState;
    [SerializeField] private TMP_Text SensorUnit;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        EventManager.OnHassStatesChanged += UpdateSensorValue;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EventManager.OnHassStatesChanged -= UpdateSensorValue;
    }

    private void UpdateSensorValue()
    {
        if (EntityState == null)
            return;
        string stateText = EntityState.state;
        Debug.Log(stateText);
        if (float.TryParse(EntityState.state, NumberStyles.Any, CultureInfo.InvariantCulture, out float stateNumber))
            stateText = stateNumber.ToString("G3");
        SensorState.text = $"{stateText}";
        SensorUnit.text = $"{EntityState.attributes.unit_of_measurement}";
    }

    protected override void UpdateEntity()
    {
        UpdateSensorValue();
    }
}
