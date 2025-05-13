using System.Collections.Generic;
using Managers;
using Structs;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A UI component that allows users to pick colors using sliders for hue, saturation, brightness, and temperature.
    /// </summary>
    public class ColorPicker : MonoBehaviour
    {
        /// <summary>
        /// Static shader property ID for the end color.
        /// </summary>
        private static readonly int ColorEnd = Shader.PropertyToID("_Color_End");

        /// <summary>
        /// Static shader property ID for alpha transparency.
        /// </summary>
        private static readonly int Alpha = Shader.PropertyToID("_Intensity");
    
        [Header("Hue")]
        [SerializeField] internal Slider HueSlider;
        [SerializeField] private GameObject HueSliderObject;
        [SerializeField] private Image HueSliderBackground;

        [Header("Saturation")]
        [SerializeField] internal Slider SaturationSlider;
        [SerializeField] private Image SaturationSliderBackground;
        [SerializeField] private GameObject SaturationSliderObject;

        [Header("Brightness")]
        [SerializeField] internal Slider BrightnessSlider;
        [SerializeField] private Image BrightnessSliderBackground;
        [SerializeField] private GameObject BrightnessSliderObject;

        [Header("Temperature")]
        [SerializeField] internal Slider TemperatureSlider;
        [SerializeField] private GameObject TemperatureSliderObject;
        [SerializeField] private Image TemperatureSliderBackground;

        /// <summary>
        /// Current hue value (0-360).
        /// </summary>
        private int _hue;

        /// <summary>
        /// Current saturation value (0-100).
        /// </summary>
        private int _saturation = 100;

        /// <summary>
        /// Current brightness value (0-255).
        /// </summary>
        private int _brightness = 255;

        /// <summary>
        /// Current color temperature value.
        /// </summary>
        private int _temperature;

        /// <summary>
        /// Whether the light is currently turned on.
        /// </summary>
        private bool _turnedOn = true;

        /// <summary>
        /// Entity ID of the light being controlled.
        /// </summary>
        private string _entityID;
        private PanelData _panelData;
    
        /// <summary>
        /// Whether the light supports color adjustment.
        /// </summary>
        private bool _supportsColor = true;

        /// <summary>
        /// Whether the light supports temperature adjustment.
        /// </summary>
        private bool _supportsTemperature;

        /// <summary>
        /// Whether there's a pending response from Home Assistant.
        /// </summary>
        private bool _hassResponsePending;

        /// <summary>
        /// Pending color change to be applied after response.
        /// </summary>
        private Color? _pendingColor;

        /// <summary>
        /// Pending brightness change to be applied after response.
        /// </summary>
        private float? _pendingBrightness;

        /// <summary>
        /// Pending temperature change to be applied after response.
        /// </summary>
        private int? _pendingTemperature;
        
        // The Slider that is currently being changed
        private Slider _currentSlider;

        private void OnEnable()
        {
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
            SetMode();
        }

        private void Start()
        {
            // Create unique material instances
            BrightnessSliderBackground.material = new Material(BrightnessSliderBackground.material);
            SaturationSliderBackground.material = new Material(SaturationSliderBackground.material);
        
            HueSlider.onValueChanged.AddListener(OnHueSliderValueChanged);
            SaturationSlider.onValueChanged.AddListener(OnSaturationSliderValueChanged);
            BrightnessSlider.onValueChanged.AddListener(OnBrightnessSliderValueChanged);
            TemperatureSlider.onValueChanged.AddListener(OnTemperatureSliderValueChanged);
        }
    
        private void OnDestroy()
        {
            HueSlider.onValueChanged.RemoveListener(OnHueSliderValueChanged);
            SaturationSlider.onValueChanged.RemoveListener(OnSaturationSliderValueChanged);
            BrightnessSlider.onValueChanged.RemoveListener(OnBrightnessSliderValueChanged);
            TemperatureSlider.onValueChanged.RemoveListener(OnTemperatureSliderValueChanged);
            EventManager.OnHassStatesChanged -= OnHassStatesChanged;
        }
        
        /// <summary>
        /// Updates the slider values when the Hass states change.
        /// </summary>
        private void OnHassStatesChanged()
        {
            _hassResponsePending = false;
            if (_pendingColor.HasValue)
            {
                ChangeColor(_pendingColor.Value);
                _pendingColor = null;
            }
            else if (_pendingBrightness.HasValue)
            {
                ChangeBrightness(_pendingBrightness.Value);
                _pendingBrightness = null;
            }
            else if (_pendingTemperature.HasValue)
            {
                ChangeTemperature(_pendingTemperature.Value);
                _pendingTemperature = null;
            }
            else
                UpdateSliderValues();
        }

        /// <summary>
        /// Handles changes to the hue slider value.
        /// </summary>
        /// <param name="value">The new hue value.</param>
        private void OnHueSliderValueChanged(float value)
        {
            if ((int)value == _hue)
                return;
            _currentSlider = HueSlider;
            _hue = (int)value;
            Color color = GetRGBColor();
            ChangeColor(color);
        }

        /// <summary>
        /// Handles changes to the saturation slider value.
        /// </summary>
        /// <param name="value">The new saturation value.</param>
        private void OnSaturationSliderValueChanged(float value)
        {
            if ((int)value == _saturation)
                return;
            _currentSlider = SaturationSlider;
            _saturation = (int)value;
            Color color = GetRGBColor();
            ChangeColor(color);
        }
    
        /// <summary>
        /// Handles changes to the brightness slider value.
        /// </summary>
        /// <param name="value">The new brightness value.</param>
        private void OnBrightnessSliderValueChanged(float value)
        {
            if ((int)value == _brightness)
                return;
            _currentSlider = BrightnessSlider;
            _brightness = (int)value;
            ChangeBrightness(value);
        }

        /// <summary>
        /// Handles changes to the temperature slider value.
        /// </summary>
        /// <param name="value">The new temperature value.</param>
        private void OnTemperatureSliderValueChanged(float value)
        {
            if ((int)value == _temperature)
                return;
            _currentSlider = TemperatureSlider;
            _temperature = (int)value;
            ChangeTemperature((int)value);
        }

        /// <summary>
        /// Changes the color of the light or demo panel based on the given color.
        /// </summary>
        /// <param name="color">The new color to set for the light or demo panel.</param>
        private void ChangeColor(Color color)
        {
            UpdateSliderBackgrounds();
            if (_panelData.IsDemoPanel) return;
            
            if (_hassResponsePending)
            {
                _pendingColor = color;
            }
            else
            {
                RestHandler.SetLightColor(_entityID, color);
                _hassResponsePending = true;
            }
        }

        /// <summary>
        /// Changes the brightness of the light or demo panel based on the provided value.
        /// </summary>
        /// <param name="value">The brightness value to set, typically between 0 and 255.</param>
        private void ChangeBrightness(float value)
        {
            UpdateSliderBackgrounds();
            if (_panelData.IsDemoPanel) return;

            if (_hassResponsePending)
            {
                _pendingBrightness = value;
            }
            else
            {
                RestHandler.SetLightBrightness(_entityID, (int)value);
                _hassResponsePending = true;
            }
        }

        /// <summary>
        /// Changes the light temperature of the light or demo color based on the temperature value.
        /// </summary>
        /// <param name="temp">The desired temperature value in Kelvin.</param>
        private void ChangeTemperature(int temp)
        {
            Color.RGBToHSV(Mathf.CorrelatedColorTemperatureToRGB(temp), out float hue, out float _, out float _);
            _hue = (int)(hue * 360);
            UpdateSliderBackgrounds();
            if (_panelData.IsDemoPanel) return;

            if (_hassResponsePending)
            {
                _pendingTemperature = temp;
            }
            else
            {
                RestHandler.SetLightTemperature(_entityID, temp);
                _hassResponsePending = true;
            }
        }

        /// <summary>
        /// Updates the sliders based on the panel state.
        /// </summary>
        /// <remarks>
        /// If the panel is off, it sets the state to false.
        /// If the panel is on, it updates the Temperature slider bounds and sets the Brightness, Saturation, and Hue sliders to the current value of the panel.
        /// </remarks>
        private void UpdateSliderValues()
        {
            HassState hassState = HassStates.GetHassState(_entityID);
            if (hassState == null)
                return;
        
            // If the panel is off, set the state to false.
            _turnedOn = hassState.state != "off";
        
            // Update Temperature slider bounds
            UpdateTemperatureSliderBounds(hassState);

            // Update Brightness slider
            _brightness = hassState.attributes.brightness;
            if (_currentSlider != BrightnessSlider)
                BrightnessSlider.SetValueWithoutNotify(_brightness);
        
            // Update Saturation slider
            _saturation = hassState.attributes.hs_color is { Length: 2 } ? (int)hassState.attributes.hs_color[1] : _saturation;
            if (_currentSlider != SaturationSlider)
                SaturationSlider.SetValueWithoutNotify(_saturation);
        
            // Update Temperature slider
            _temperature = hassState.attributes.color_temp_kelvin;
            if (_currentSlider != TemperatureSlider)
                TemperatureSlider.SetValueWithoutNotify(_temperature);

            // Update Hue slider if saturation is greater than 10%, to avoid the hue slider from jumping to a wrong value
            if (_saturation > 0.1f)
            {
                _hue = hassState.attributes.hs_color is { Length: 2 } ? (int)hassState.attributes.hs_color[0] : _hue;
                if (_currentSlider != HueSlider)
                    HueSlider.SetValueWithoutNotify(_hue);
            }

            _currentSlider = null;
            UpdateSliderBackgrounds();
        }
    
        /// <summary>
        /// Updates the temperature slider bounds with the min and max color temp kelvin of the panel.
        /// </summary>
        /// <param name="hassState">The HassState object of the panel.</param>
        private void UpdateTemperatureSliderBounds(HassState hassState)
        {
            if ((int)TemperatureSlider.minValue == hassState.attributes.min_color_temp_kelvin && (int)TemperatureSlider.maxValue == hassState.attributes.max_color_temp_kelvin)
                return;
            // Temporally remove the listener to avoid triggering the event when setting min and max values
            TemperatureSlider.onValueChanged.RemoveListener(OnTemperatureSliderValueChanged);
            TemperatureSlider.minValue = hassState.attributes.min_color_temp_kelvin;
            TemperatureSlider.maxValue = hassState.attributes.max_color_temp_kelvin;
            TemperatureSlider.onValueChanged.AddListener(OnTemperatureSliderValueChanged);
        }

        /// <summary>
        /// Converts the current hue, saturation, and brightness values to an RGB color.
        /// </summary>
        /// <returns>The RGB color.</returns>
        private Color GetRGBColor()
        {
            return Color.HSVToRGB(_hue / 360f, _saturation / 100f, _brightness / 255f);
        }

        /// <summary>
        /// Updates the slider backgrounds based on the current state of the hue, saturation and brightness sliders.
        /// </summary>
        private void UpdateSliderBackgrounds()
        {
            // Update Saturation background color
            Color color = Color.HSVToRGB(_hue / 360f, 1f, _brightness / 255f);
            SaturationSliderBackground.materialForRendering.SetColor(ColorEnd, color);

            switch (_supportsColor)
            {
                // Update Brightness background color
                case false when _supportsTemperature:
                    BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, Mathf.CorrelatedColorTemperatureToRGB(_temperature));
                    break;
                case true:
                    color = Color.HSVToRGB(_hue / 360f, _saturation / 100f, 1f);
                    BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, color);
                    break;
                default:
                    BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, Color.white);
                    break;
            }
        
            // Set the alpha to 0.5 if the panel is off
            float alpha = _turnedOn == false ? 0.5f : 1f;
            SaturationSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
            BrightnessSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
            TemperatureSliderBackground.color = new Color(1,1,1, alpha);
            HueSliderBackground.color = new Color(1,1,1, alpha);
        }

        /// <summary>
        /// Sets the panel data for the color picker, updates internal properties, and refreshes slider values.
        /// </summary>
        /// <param name="panelData">The data object containing panel information.</param>
        public void SetPanelData(PanelData panelData)
        {
            _panelData = panelData;
            _entityID = panelData.EntityID;
            SetMode();
            UpdateSliderValues();
        }

        /// <summary>
        /// Toggles the color picker sliders based on the supported color modes of the panel.
        /// </summary>
        private void SetMode()
        { 
            if (_panelData == null)
                return;

            if (!CheckColorSupport(out HashSet<string> modes))
            {
                gameObject.SetActive(false);
                return;
            }
            if (_panelData.IsDemoPanel)
                return;

            // All remaining cases support the brightness mode, so it can stay on
        
            // If the panel supports the color_temp mode, show the temperature slider
            _supportsTemperature = modes.Contains("color_temp");
            TemperatureSliderObject.SetActive(_supportsTemperature);
        
            // If the panel supports the hs, rgb, rgbw, rgbww, white, or xy modes, show the color picker
            _supportsColor = modes.Overlaps(new[] { "hs", "rgb", "rgbw", "rgbww", "white", "xy" });
            HueSliderObject.SetActive(_supportsColor);
            SaturationSliderObject.SetActive(_supportsColor);
        }

        /// <summary>
        /// Checks if the color picker supports color and temperature modes for the given panel data entity.
        /// </summary>
        /// <param name="modes">
        /// An output parameter that will contain the set of supported color modes if the device supports them.
        /// </param>
        /// <returns>
        /// A boolean value indicating whether the device supports any color or temperature modes.
        /// </returns>
        private bool CheckColorSupport(out HashSet<string> modes)
        {
            modes = new HashSet<string>();
            
            if (HassStates.GetDeviceType(_panelData.EntityID) != EDeviceType.LIGHT)
                return false;
            if (_panelData.IsDemoPanel)
                return true;
            if (HassStates.GetHassState(_panelData.EntityID) == null)
                return false;
            
            modes = new HashSet<string>(HassStates.GetHassState(_panelData.EntityID).attributes.supported_color_modes);
        
            // If the panel only supports the onoff mode, hide the color picker
            if (modes.Count == 0 || modes.Count == 1 && modes.Contains("onoff") || modes.Contains("unknown"))
                return false;

            return true;
        }
    }
}