
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Managers;
using UnityEngine;
using UnityEngine.UI;

public abstract class MaterialDesignIcons
{
    private static CodepointData[] _codepointsCollection;

    public static string GetIcon(HassState entity)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();
        if (entity.attributes == null)
            return GetIconByName("help");

        string iconName;

        if (string.IsNullOrEmpty(entity.attributes.icon))
        {
            switch (entity.DeviceType)
            {
                case EDeviceType.DEFAULT:
                    iconName = "help";
                    break;
                case EDeviceType.CLIMATE:
                    iconName = "thermostat";
                    break;
                case EDeviceType.LIGHT:
                    iconName = "lightbulb";
                    break;
                case EDeviceType.SWITCH:
                    iconName = "toggle-switch-variant-off";
                    break;
                case EDeviceType.CALENDAR:
                    iconName = "calendar";
                    break;
                case EDeviceType.SENSOR:
                    Enum.TryParse(entity.attributes.device_class, true, out ESensorDeviceClass sensorClass);
                    iconName = sensorClass switch
                    {
                        ESensorDeviceClass.ENERGY or ESensorDeviceClass.POWER => "lightning-bolt",
                        ESensorDeviceClass.TEMPERATURE => "thermometer",
                        _ => "radar"
                    };
                    break;
                case EDeviceType.BINARY_SENSOR:
                    iconName = entity.state == "on" ? "check-circle" : "circle-outline";
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(entity), entity, null);
            }
        }
        else
        {
            iconName = entity.attributes.icon.Split(":")[1];
        }

        return (from data in _codepointsCollection where data.Name == iconName select data.Code).FirstOrDefault();
    }

    public static string GetIconByName(string mdiName)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();
        return (from data in _codepointsCollection where data.Name == mdiName select data.Code).FirstOrDefault();
    }
    

    private static void InitiateCodepointsCollection()
    {
        TextAsset mdi = Resources.Load<TextAsset>("codepoints");
        string[] codepoints = mdi.text.Split('\n');
        _codepointsCollection = codepoints
            .Select(codepoint => new CodepointData(codepoint))
            .Where(data => data.Code != null) // Exclude invalid entries
            .ToArray();
    }
    
    [System.Serializable]
    public class CodepointData
    {
        public string Name { get; private set; }
        public string Hex { get; private set; }
        public string Code { get; private set; }

        public CodepointData(string codepoint)
        {
            string[] data = codepoint.Split(' ');
            Name = data[0];
            Hex = data[1];

            // Convert hex to a Unicode character using \U escape sequence
            int unicodeValue = int.Parse(Hex, System.Globalization.NumberStyles.HexNumber);
            Code = char.ConvertFromUtf32(unicodeValue); // Supports supplementary planes
        }
    }

    public static string GetWeatherIcon(string forecastCondition)
    {
        string iconName = WeatherIcons.GetValueOrDefault(forecastCondition, "");
        return GetIconByName(iconName);
    }
    
    /// <summary>
    /// Gets the Material Design icon for the given weather condition.
    /// </summary>
    /// <returns>The Material Design icon for the given weather condition.</returns>
    private static readonly Dictionary<string, string> WeatherIcons = new()
    {
        { "clear-night", "weather-night" },
        { "cloudy", "weather-cloudy" },
        { "exceptional", "alert-circle-outline" },
        { "fog", "weather-fog" },
        { "hail", "weather-hail" },
        { "lightning", "weather-lightning" },
        { "lightning-rainy", "weather-lightning-rainy" },
        { "partlycloudy", "weather-partly-cloudy" },
        { "pouring", "weather-pouring" },
        { "rainy", "weather-rainy" },
        { "snowy", "weather-snowy" },
        { "snowy-rainy", "weather-snowy-rainy" },
        { "sunny", "weather-sunny" },
        { "windy", "weather-windy" },
        { "windy-variant", "weather-windy-variant" }
    };
}
