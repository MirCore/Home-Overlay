using System;
using Managers;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class ColorPicker : MonoBehaviour
{
    private static readonly int ColorEnd = Shader.PropertyToID("_Color_End");
    private static readonly int Alpha = Shader.PropertyToID("_Alpha");
    [SerializeField] internal Slider HueSlider;
    [SerializeField] internal Slider SaturationSlider;
    [SerializeField] private Image SaturationSliderBackground;
    [SerializeField] internal Slider BrightnessSlider;
    [SerializeField] private Image BrightnessSliderBackground;

    private int _hue;
    private int _saturation;
    private int _brightness;
    private bool _turnedOn;
    private Color _color;
    private string _entityID;

    private void OnEnable()
    {
        HueSlider.onValueChanged.AddListener(OnHueSliderValueChanged);
        SaturationSlider.onValueChanged.AddListener(OnSaturationSliderValueChanged);
        BrightnessSlider.onValueChanged.AddListener(OnBrightnessSliderValueChanged);
        EventManager.OnHassStatesChanged += OnHassStatesChanged;
    }
    
    private void OnDisable()
    {
        HueSlider.onValueChanged.RemoveListener(OnHueSliderValueChanged);
        SaturationSlider.onValueChanged.RemoveListener(OnSaturationSliderValueChanged);
        BrightnessSlider.onValueChanged.RemoveListener(OnBrightnessSliderValueChanged);
        EventManager.OnHassStatesChanged -= OnHassStatesChanged;
    }

    private void OnHassStatesChanged()
    {
        UpdateSliders();
        UpdateColor();
    }

    private void OnHueSliderValueChanged(float value)
    {
        _hue = (int)value;
        UpdateColor();
        RestHandler.SetLightColor(_entityID, _color);
    }
    
    private void OnSaturationSliderValueChanged(float value)
    {
        _saturation = (int)value;
        UpdateColor();
        RestHandler.SetLightColor(_entityID, _color);
    }
    
    private void OnBrightnessSliderValueChanged(float value)
    {
        _brightness = (int)value;
        UpdateColor();
        RestHandler.SetLightBrightness(_entityID, (int)value);
    }
    
    private void UpdateColor()
    {
        _color = Color.HSVToRGB(_hue / 360f, _saturation / 100f, _brightness / 255f);
        
        Color hue = Color.HSVToRGB(_hue / 360f, 1f, 1f);
        SaturationSliderBackground.materialForRendering.SetColor(ColorEnd, hue);
        BrightnessSliderBackground.materialForRendering.SetColor(ColorEnd, hue);

        float alpha = _turnedOn == false ? 0.5f : 1f;
        SaturationSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
        BrightnessSliderBackground.materialForRendering.SetFloat(Alpha, alpha);
    }

    private void UpdateSliders()
    {
        HassEntity entityState = GameManager.Instance.GetHassState(_entityID);
        if (entityState == null)
            return;

        float hue = _hue;
        float saturation = _saturation;
        float brightness = entityState.attributes.brightness;
        
        // Update the icon color based on the entity's state.
        // If the entity is off, set the state to false.
        if (entityState.state == "off")
        {
            _turnedOn = false;
            return;
        }
        _turnedOn = true;

        // If the entity has a valid hs color, set the icon color to it.
        if (entityState.attributes.hs_color is { Length: 2 })
        {
            hue = entityState.attributes.hs_color[0];
            saturation = entityState.attributes.hs_color[1];
        }
        
        if (saturation > 0.1f)
            HueSlider.SetValueWithoutNotify(hue);
        SaturationSlider.SetValueWithoutNotify(saturation);
        BrightnessSlider.SetValueWithoutNotify(brightness);
        
        if (saturation > 0.1f)
            _hue = (int)hue;
        _saturation = (int)saturation;
        _brightness = (int)brightness;
    }

    public void SetEntityID(string entityID)
    {
        _entityID = entityID;
    }
}
