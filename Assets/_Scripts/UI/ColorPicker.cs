using System.Collections.Generic;
using Managers;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    /// <summary>
    /// A UI component that allows users to pick colors using sliders for hue, saturation, brightness, and temperature.
    /// </summary>
    public class ColorPicker : MonoBehaviour
    {
        private static readonly int ColorEnd = Shader.PropertyToID("_Color_End");
        private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    
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

        private int _hue;
        private int _saturation = 100;
        private int _brightness = 255;
        private int _temperature;
        private bool _turnedOn;
        private string _entityID;
    
        private bool _supportsColor;
        private bool _supportsTemperature;
        
        // The Slider that is currently being changed
        private Slider _currentSlider;

        private void OnEnable()
        {
            EventManager.OnHassStatesChanged += OnHassStatesChanged;
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
            RestHandler.SetLightColor(_entityID, color);
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
            RestHandler.SetLightColor(_entityID, color);
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
            RestHandler.SetLightBrightness(_entityID, (int)value);
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
            RestHandler.SetLightTemperature(_entityID, _temperature);
        }
    
        /// <summary>
        /// Updates the sliders based on the panel state.
        /// </summary>
        /// <remarks>
        /// If the panel is off, it sets the state to false.
        /// If the panel is on, it updates the Temperature slider bounds, and sets the Brightness, Saturation and Hue sliders to the current value of the panel.
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
            Color hue = Color.HSVToRGB(_hue / 360f, 1f, 1f);
            SaturationSliderBackground.materialForRendering.SetColor(ColorEnd, hue);
        
            // Update Brightness background color
            if (!_supportsColor && _supportsTemperature)
                BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, Mathf.CorrelatedColorTemperatureToRGB(_temperature));
            else if (_supportsColor)
                BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, hue);
            else
                BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, Color.white);
        
            // Set the alpha to 0.5 if the panel is off
            float alpha = _turnedOn == false ? 0.5f : 1f;
            SaturationSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
            BrightnessSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
            TemperatureSliderBackground.color = new Color(1,1,1, alpha);
            HueSliderBackground.color = new Color(1,1,1, alpha);
        }

        /// <summary>
        /// Sets the entity ID for the color picker.
        /// </summary>
        /// <param name="entityID">The entity ID.</param>
        public void SetEntityID(string entityID)
        {
            _entityID = entityID;
            UpdateSliderValues();
        }
    
        /// <summary>
        /// Toggles the color picker sliders based on the supported color modes of the panel.
        /// </summary>
        /// <param name="supportedColorModes">The supported color modes of the panel.</param>
        public void SetMode(string[] supportedColorModes)
        { 
            HashSet<string> modes = new (supportedColorModes);
        
            // If the panel only supports the onoff mode, hide the color picker
            if (modes.Count == 1 && modes.Contains("onoff") || modes.Contains("unknown"))
            {
                gameObject.SetActive(false);
                return;
            }
        
            // All remaining cases support the brightness mode, so it can stay on
        
            // If the panel supports the color_temp mode, show the temperature slider
            _supportsTemperature = modes.Contains("color_temp");
            TemperatureSliderObject.SetActive(_supportsTemperature);
        
            // If the panel supports the hs, rgb, rgbw, rgbww, white, or xy modes, show the color picker
            _supportsColor = modes.Overlaps(new[] { "hs", "rgb", "rgbw", "rgbww", "white", "xy" });
            HueSliderObject.SetActive(_supportsColor);
            SaturationSliderObject.SetActive(_supportsColor);
        }
    }
}
