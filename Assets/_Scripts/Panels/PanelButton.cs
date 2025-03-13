using System;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Panel
{
    public class PanelButton : Panels.Panel
    {
        [SerializeField] private Button Button;
        
    
        protected override void OnEnable()
        {
            base.OnEnable();
        
            Button.onClick.AddListener(OnButtonClicked);
        }

        protected override void UpdatePanel()
        {
            UpdateIcon();
        }

        /// <summary>
        /// Toggles the entity associated with the panel ID.
        /// </summary>
        private void OnButtonClicked()
        {
            if (!PanelIsReady())
                return;
        
            switch (HassState.DeviceType)
            {
                case EDeviceType.DEFAULT:
                    break;
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
        /// Updates the icon color based on the panel's state and attributes.
        /// </summary>
        private void UpdateIcon()
        {
            if (!PanelIsReady())
                return;
        
            // Update the icon based on the panel's attributes.
            Icon.text = MaterialDesignIcons.GetIcon(HassState);

            Color color;
        
            // Update the icon color based on the panel's state.
            // If the panel is off, set the icon color to black.
            if (HassState.state == "off")
            {
                color = Color.black;
            }
            // If the panel has a valid RGB color, set the icon color to it.
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