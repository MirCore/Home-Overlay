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
    
        /// <summary>
        /// Updates the icon color based on the entity's state and attributes.
        /// </summary>
        private void UpdateIcon()
        {
            if (EntityState == null)
                return;
        
            // Update the icon based on the entity's attributes.
            Icon.text = MaterialDesignIcons.GetIcon(EntityState);

            Color color;
        
            // Update the icon color based on the entity's state.
            // If the entity is off, set the icon color to black.
            if (EntityState.state == "off")
            {
                color = Color.black;
            }
            // If the entity has a valid RGB color, set the icon color to it.
            else if (EntityState.attributes is { rgb_color: { Length: 3 } })
            {
                color = JsonHelpers.RGBToUnityColor(EntityState.attributes.rgb_color);
            }
            // Otherwise, set the icon color to white.
            else
            {
                color = Color.white;
            }

            if (EntityState.attributes != null && EntityState.attributes.brightness != 0)
            {
                color = Color.Lerp(Color.black, color, EntityState.attributes.brightness / 255f);
            }
        
            Icon.color = color;
        }
    }
}