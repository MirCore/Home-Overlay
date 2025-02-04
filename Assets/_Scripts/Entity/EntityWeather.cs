using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using Utils;

namespace Entity
{
    public class EntityWeather : Entity
    {
        [SerializeField] private TMP_Text State;
        [SerializeField] private TMP_Text Temperature;
        [SerializeField] private ForecastPanel ForecastPanel;
        
        [SerializeField] private List<WeatherForecast> InspectorWeatherForecast;
        
        [SerializeField] private int MaxForecastPanels = 5;
        
        /// <summary>
        /// How often to refresh the weather forecast in seconds
        /// </summary>
        [Tooltip("How often to refresh the weather forecast in seconds")]
        [SerializeField] private float WeatherForecastRefreshRate = 600f;
        
        private readonly List<ForecastPanel> _forecastPanels = new ();
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private void Start()
        {
            _forecastPanels.Add(ForecastPanel);
            
            if (HassState != null)
            {
                _cancellationTokenSource = new CancellationTokenSource();
                _ = GetHassWeatherForecast(_cancellationTokenSource.Token);
            }
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
        }

        /// <summary>
        /// Updates the weather forecast periodically.
        /// </summary>
        /// <param name="token">The cancellation token to cancel the task.</param>
        private async Task GetHassWeatherForecast(CancellationToken token)
        {
            if (HassState == null)
                _cancellationTokenSource.Cancel();
            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (HassState != null) 
                        RestHandler.GetWeatherForecast(HassState.entity_id, UpdateWeather);
                }
                catch (TaskCanceledException)
                {
                    //Debug.Log("Weather update task was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while updating weather forecast: {ex.Message}");
                }
                
                await Task.Delay(TimeSpan.FromSeconds(WeatherForecastRefreshRate), token);
            }
        }

        /// <summary>
        /// Updates the current weather state and forecast.
        /// </summary>
        /// <param name="forecastResponse">The forecast response from Home Assistant.</param>
        private void UpdateWeather(string forecastResponse)
        {
            UpdateCurrentWeather();
            UpdateForecast(forecastResponse);
        }

        /// <summary>
        /// Updates the current weather state.
        /// </summary>
        private void UpdateCurrentWeather()
        {
            State.text = StringManipulation.CapitalizeFirstLetter(HassState.state);
            Temperature.text = $"{HassState.attributes.temperature} {HassState.attributes.temperature_unit}";
            Icon.text = MaterialDesignIcons.GetIconByName(WeatherIcons.GetValueOrDefault(HassState.state, ""));
        }

        /// <summary>
        /// Updates the forecast.
        /// </summary>
        /// <param name="forecastResponse">The forecast response from Home Assistant.</param>
        private void UpdateForecast(string forecastResponse)
        {
            List<WeatherForecast> weatherForecast = HassStates.ConvertHassWeatherResponse(forecastResponse);
            
            // For debugging
            InspectorWeatherForecast = weatherForecast;
            
            if (weatherForecast == null)
                return;
            
            int numberOfForecasts = Math.Min(MaxForecastPanels, weatherForecast.Count);
            
            ActivatePanels(numberOfForecasts);

            for (int i = 0; i < numberOfForecasts; i++)
            {
                ForecastPanel forecastPanel = _forecastPanels[i];
                WeatherForecast forecast = weatherForecast[i];
                
                // Parse the string into a DateTime object (handling the time zone correctly), then format it as a short day name (e.g., "Mon").
                string dayOfWeek = DateTime.Parse(forecast.datetime, null, DateTimeStyles.RoundtripKind).ToString("ddd", CultureInfo.InvariantCulture);
                
                forecastPanel.Date.text = dayOfWeek;
                forecastPanel.MaxTemp.text = $"{forecast.temperature}°";
                forecastPanel.MinTemp.text = $"{forecast.templow}°";
                forecastPanel.Icon.text = MaterialDesignIcons.GetIconByName(WeatherIcons.GetValueOrDefault(forecast.condition, ""));
            }
        }

        /// <summary>
        /// Activates or deactivates forecast panels based on the number of forecasts.
        /// </summary>
        /// <param name="numberOfForecasts">The number of forecasts to display.</param>
        private void ActivatePanels(int numberOfForecasts)
        {
            while (_forecastPanels.Count < numberOfForecasts)
            {
                ForecastPanel newPanel = Instantiate(ForecastPanel, ForecastPanel.transform.parent);
                _forecastPanels.Add(newPanel);
            }
            
            for (int i = 0; i < _forecastPanels.Count; i++)
            {
                _forecastPanels[i].gameObject.SetActive(i < numberOfForecasts);
            }
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

        /// <summary>
        /// Called when the entity state changes.
        /// </summary>
        protected override void UpdateEntity()
        {
            if (HassState == null)
                return;
            UpdateCurrentWeather();
            
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
                return;  // Prevent multiple tasks

            _cancellationTokenSource?.Cancel(); // Cancel any existing token
            _cancellationTokenSource?.Dispose(); // Dispose of the old CTS

            _cancellationTokenSource = new CancellationTokenSource();
            _ = GetHassWeatherForecast(_cancellationTokenSource.Token);
        }
    }
}