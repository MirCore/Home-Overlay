using System;
using UnityEngine;
using UnityEngine.UI;

public class EntityButton : Entity
{
    [SerializeField] private Button Button;
        
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        Button.onClick.AddListener(OnButtonClicked);
    }

    /// <summary>
    /// Toggles the entities state associated with the entity ID.
    /// </summary>
    private void OnButtonClicked()
    {
        if (EntityState == null)
            return;
        
        switch (EntityState.DeviceType)
        {
            case EDeviceType.DEFAULT:
                break;
            case EDeviceType.LIGHT:
                RestHandler.ToggleLight(EntityObject.EntityID);
                break;
            case EDeviceType.SWITCH:
                RestHandler.ToggleSwitch(EntityObject.EntityID);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}