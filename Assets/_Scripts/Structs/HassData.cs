using System;
using System.Diagnostics.CodeAnalysis;

namespace Structs
{
    /// <summary>
    /// Represents the state of an entity in Home Assistant, including its attributes and device type.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HassState
    {
        /// <summary>
        /// The unique identifier of the entity (e.g., "light.living_room", "sensor.temperature").
        /// </summary>
        public string entity_id;

        /// <summary>
        /// The current state of the entity (e.g., "on", "off", "23.5").
        /// </summary>
        public string state = "";

        /// <summary>
        /// Additional attributes that describe the entity's properties and capabilities.
        /// </summary>
        public HassEntityAttributes attributes;

        /// <summary>
        /// The type of device this entity represents (e.g., LIGHT, SENSOR, SWITCH).
        /// </summary>
        public EDeviceType DeviceType;

        /// <summary>
        /// Contains various attributes that describe an entity's properties, capabilities, and current values.
        /// </summary>
        [Serializable]
        public class HassEntityAttributes
        {
            /// <summary>
            /// The unit of measurement for sensor values (e.g., "°C", "lux", "%").
            /// </summary>
            public string unit_of_measurement;

            /// <summary>
            /// The class of the device, indicating its primary function (e.g., "temperature", "humidity").
            /// </summary>
            public string device_class;

            /// <summary>
            /// The minimum color temperature supported by the light, in Kelvin.
            /// </summary>
            public int min_color_temp_kelvin;

            /// <summary>
            /// The maximum color temperature supported by the light, in Kelvin.
            /// </summary>
            public int max_color_temp_kelvin;

            /// <summary>
            /// Array of color modes supported by the light (e.g., ["rgb", "color_temp"]).
            /// </summary>
            public string[] supported_color_modes;

            /// <summary>
            /// The current color mode of the light.
            /// </summary>
            public string color_mode;

            /// <summary>
            /// The current brightness level (0-255).
            /// </summary>
            public int brightness;

            /// <summary>
            /// The current color temperature in Kelvin.
            /// </summary>
            public int color_temp_kelvin;

            /// <summary>
            /// The current color temperature in mireds.
            /// </summary>
            public int color_temp;

            /// <summary>
            /// The current RGB color values [red, green, blue] (0-255).
            /// </summary>
            public int[] rgb_color;

            /// <summary>
            /// The current color in HSV format [hue, saturation] (hue: 0-360, saturation: 0-100).
            /// </summary>
            public float[] hs_color;

            /// <summary>
            /// The Material Design icon identifier for this entity.
            /// </summary>
            public string icon;

            /// <summary>
            /// The human-readable name of the entity.
            /// </summary>
            public string friendly_name;

            /// <summary>
            /// The current temperature reading for climate entities.
            /// </summary>
            public float current_temperature;

            /// <summary>
            /// The target temperature setting for climate entities.
            /// </summary>
            public float temperature;

            /// <summary>
            /// The unit used for temperature values (e.g., "°C", "°F").
            /// </summary>
            public string temperature_unit;
        }
    }

    /// <summary>
    /// Represents the Home Assistant configuration, including localization and unit settings.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class HassConfig
    {
        /// <summary>
        /// The country code where the Home Assistant instance is located.
        /// </summary>
        public string country;

        /// <summary>
        /// The currency used by the Home Assistant instance.
        /// </summary>
        public string currency;

        /// <summary>
        /// The language code used by the Home Assistant instance.
        /// </summary>
        public string language;

        /// <summary>
        /// The time zone of the Home Assistant instance.
        /// </summary>
        public string time_zone;

        /// <summary>
        /// The unit system configuration used by Home Assistant.
        /// </summary>
        public UnitSystem unit_system;

        /// <summary>
        /// The version of Home Assistant.
        /// </summary>
        public string version;

        /// <summary>
        /// Represents the unit configuration in Home Assistant.
        /// </summary>
        [Serializable]
        public class UnitSystem
        {
            /// <summary>
            /// The unit used for length measurements (e.g., "km", "mi").
            /// </summary>
            public string length;

            /// <summary>
            /// The unit used for accumulated precipitation measurements (e.g., "mm", "in").
            /// </summary>
            public string accumulated_precipitation;

            /// <summary>
            /// The unit used for area measurements (e.g., "m²", "ft²").
            /// </summary>
            public string area;

            /// <summary>
            /// The unit used for mass measurements (e.g., "kg", "lb").
            /// </summary>
            public string mass;

            /// <summary>
            /// The unit used for pressure measurements (e.g., "hPa", "inHg").
            /// </summary>
            public string pressure;

            /// <summary>
            /// The unit used for temperature measurements (e.g., "°C", "°F").
            /// </summary>
            public string temperature;

            /// <summary>
            /// The unit used for volume measurements (e.g., "L", "gal").
            /// </summary>
            public string volume;

            /// <summary>
            /// The unit used for wind speed measurements (e.g., "m/s", "mph").
            /// </summary>
            public string wind_speed;
        }
    }

    /// <summary>
    /// Represents a weather forecast from Home Assistant.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class WeatherForecast
    {
        /// <summary>
        /// The weather condition (e.g., "sunny", "cloudy", "rain").
        /// </summary>
        public string condition;

        /// <summary>
        /// The probability of precipitation as a percentage (0-100).
        /// </summary>
        public double precipitation_probability;

        /// <summary>
        /// The date and time of the forecast.
        /// </summary>
        public string datetime;

        /// <summary>
        /// The wind direction in degrees (0-360).
        /// </summary>
        public double wind_bearing;

        /// <summary>
        /// The UV index value.
        /// </summary>
        public double uv_index;

        /// <summary>
        /// The forecasted temperature.
        /// </summary>
        public double temperature;

        /// <summary>
        /// The forecasted minimum temperature.
        /// </summary>
        public double templow;

        /// <summary>
        /// The wind gust speed in configured units.
        /// </summary>
        public double wind_gust_speed;

        /// <summary>
        /// The wind speed in configured units.
        /// </summary>
        public double wind_speed;

        /// <summary>
        /// The amount of precipitation in configured units.
        /// </summary>
        public double precipitation;

        /// <summary>
        /// The relative humidity percentage (0-100).
        /// </summary>
        public int humidity;
    }

    /// <summary>
    /// Represents a calendar event from Home Assistant.
    /// </summary>
    [Serializable]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public class CalendarEvent
    {
        /// <summary>
        /// The start date and time of the event.
        /// </summary>
        public EventDate start;

        /// <summary>
        /// The end date and time of the event.
        /// </summary>
        public EventDate end;

        /// <summary>
        /// The event summary or title.
        /// </summary>
        public string summary;

        /// <summary>
        /// The detailed description of the event.
        /// </summary>
        public string description;

        /// <summary>
        /// The location where the event takes place.
        /// </summary>
        public string location;

        /// <summary>
        /// Represents a date/time value that can be either a full datetime or just a date.
        /// </summary>
        [Serializable]
        public class EventDate
        {
            /// <summary>
            /// The date component in ISO format (YYYY-MM-DD) for all-day events.
            /// </summary>
            public string date;

            /// <summary>
            /// The full date and time in ISO format for events with specific times.
            /// </summary>
            public string dateTime;

            /// <summary>
            /// Converts the date or datetime string to a DateTime object.
            /// </summary>
            /// <returns>A DateTime object if either date or dateTime is valid; null otherwise.</returns>
            public DateTime? GetDateTime()
            {
                if (!string.IsNullOrEmpty(dateTime))
                    return DateTime.Parse(dateTime);
                if (!string.IsNullOrEmpty(date))
                    return DateTime.Parse(date);
                return null;
            }
        }
    }
}