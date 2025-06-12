using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Structs;
using UnityEngine;

public abstract class MaterialDesignIcons
{
    private static CodepointData[] _codepointsCollection;

    /// <summary>
    /// Retrieves the icon corresponding to the provided Home Assistant entity.
    /// The method uses the properties of the entity, including its device type and state, to determine the most appropriate Material Design Icon.
    /// If an icon is explicitly specified within the entity attributes, that icon will be used.
    /// </summary>
    /// <param name="entity">The Home Assistant entity from which the icon is determined.</param>
    /// <returns>The Material Design Icon codepoint as a string corresponding to the entity's characteristics.</returns>
    public static string GetIcon(HassState entity)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();
        if (entity.attributes == null)
            return GetIcon("help");

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
    
    /// <summary>
    /// Retrieves the Material Design Icon codepoint for the specified icon name.
    /// </summary>
    /// <param name="mdiName">The name of the Material Design Icon to retrieve.</param>
    /// <returns>The Unicode codepoint for the specified icon as a string. Returns null if the icon name is not found.</returns>
    public static string GetIcon(string mdiName)
    {
        if (_codepointsCollection == null)
            InitiateCodepointsCollection();
        return (from data in _codepointsCollection where data.Name == mdiName select data.Code).FirstOrDefault();
    }


    /// <summary>
    /// Initializes the collection of Material Design Icon codepoints by loading data from a resource.
    /// This method reads a text file containing the icon codepoints, processes them into a usable format,
    /// and stores the result in an internal collection for later access.
    /// </summary>
    private static void InitiateCodepointsCollection()
    {
        TextAsset mdi = Resources.Load<TextAsset>("codepoints");
        string[] codepoints = mdi.text.Split('\n');
        _codepointsCollection = codepoints
            .Select(codepoint => new CodepointData(codepoint))
            .Where(data => data.Code != null) // Exclude invalid entries
            .ToArray();
    }

    /// <summary>
    /// Represents a data record for a Material Design Icon codepoint.
    /// Contains information about the icon's name, its hexadecimal Unicode value, and its corresponding character code.
    /// </summary>
    [Serializable]
    public class CodepointData
    {
        /// <summary>
        /// Gets the name identifier of the Material Design Icon.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets the hexadecimal Unicode value of the icon character.
        /// </summary>
        public string Hex { get; private set; }

        /// <summary>
        /// Gets the actual Unicode character representation of the icon.
        /// </summary>
        public string Code { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodepointData"/> class.
        /// </summary>
        /// <param name="codepoint">A space-separated string containing the icon name and its hexadecimal Unicode value.</param>
        public CodepointData(string codepoint)
        {
            string[] data = codepoint.Split(' ');
            Name = data[0];
            Hex = data[1];

            // Convert hex to a Unicode character using \U escape sequence
            int unicodeValue = int.Parse(Hex, NumberStyles.HexNumber);
            Code = char.ConvertFromUtf32(unicodeValue); // Supports supplementary planes
        }
    }

    /// <summary>
    /// Retrieves the Material Design weather icon name corresponding to the provided forecast condition and returns its icon representation.
    /// </summary>
    /// <param name="forecastCondition">The weather condition string (e.g., "cloudy", "sunny") to map to a Material Design icon name.</param>
    /// <returns>A string representing the weather icon. Returns an empty string if no mapping is found for the provided condition.</returns>
    public static string GetWeatherIcon(string forecastCondition)
    {
        string iconName = WeatherIcons.GetValueOrDefault(forecastCondition, "");
        return GetIcon(iconName);
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