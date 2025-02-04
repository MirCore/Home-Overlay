using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Entity
{
    public class EntityButton : Entity
    {
        [SerializeField] private Button Button;
        
    
        protected override void OnEnable()
        {
            base.OnEnable();
        
            Button.onClick.AddListener(OnButtonClicked);
        }

        protected override void UpdateEntity()
        {
            UpdateIcon();
        }

        /// <summary>
        /// Toggles the entities state associated with the entity ID.
        /// </summary>
        private void OnButtonClicked()
        {
            if (HassState == null)
                return;
        
            switch (HassState.DeviceType)
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
    
        /// <summary>
        /// Updates the icon color based on the entity's state and attributes.
        /// </summary>
        private void UpdateIcon()
        {
            if (HassState == null)
                return;
        
            // Update the icon based on the entity's attributes.
            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            Color color;
        
            // Update the icon color based on the entity's state.
            // If the entity is off, set the icon color to black.
            if (HassState.state == "off")
            {
                color = Color.black;
            }
            // If the entity has a valid RGB color, set the icon color to it.
            else if (HassState.attributes is { rgb_color: { Length: 3 } })
            {
                color = JsonHelpers.RGBToUnityColor(HassState.attributes.rgb_color);
            }
            // Otherwise, set the icon color to white.
            else
            {
                color = Color.white;
            }

            if (HassState.attributes != null && HassState.attributes.brightness != 0)
            {
                color = Color.Lerp(Color.black, color, HassState.attributes.brightness / 255f);
            }
        
            Icon.color = color;
        }
    }
}