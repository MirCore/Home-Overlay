using System;
using System.Globalization;
using Structs;
using TMPro;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// A UI component that displays a single weather forecast.
    /// </summary>
    public class ForecastField : MonoBehaviour
    {
        /// <summary>
        /// The text component for displaying the forecast date.
        /// </summary>
        [SerializeField] internal TMP_Text Date;

        /// <summary>
        /// The text component for displaying the forecast icon.
        /// </summary>
        [SerializeField] internal TMP_Text Icon;

        /// <summary>
        /// The text component for displaying the maximum temperature.
        /// </summary>
        [SerializeField] internal TMP_Text MaxTemp;

        /// <summary>
        /// The text component for displaying the minimum temperature.
        /// </summary>
        [SerializeField] internal TMP_Text MinTemp;

        /// <summary>
        /// Updates the forecast field with the provided weather forecast data.
        /// </summary>
        /// <param name="forecast">The weather forecast data.</param>
        public void UpdateForecast(WeatherForecast forecast)
        {
            // Parses the string into a DateTime object, then format it as a short day name (e.g., "Mon").
            string dayOfWeek = DateTime.Parse(forecast.datetime, null, DateTimeStyles.RoundtripKind).ToString("ddd", CultureInfo.InvariantCulture);

            Date.text = dayOfWeek;
            MaxTemp.text = $"{forecast.temperature}°";
            MinTemp.text = $"{forecast.templow}°";
            Icon.text = MaterialDesignIcons.GetWeatherIcon(forecast.condition);
        }
    }
}