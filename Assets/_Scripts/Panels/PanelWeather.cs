using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Structs;
using TMPro;
using UI;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Panels
{
    /// <summary>
    /// A panel that displays weather information and forecasts.
    /// </summary>
    public class PanelWeather : Panel
    {
        /// <summary>
        /// The text component for displaying the current temperature.
        /// </summary>
        [SerializeField] private TMP_Text Temperature;

        /// <summary>
        /// The forecast field component.
        /// </summary>
        [SerializeField] private ForecastField ForecastField;

        /// <summary>
        /// The maximum number of forecast panels to display.
        /// </summary>
        [SerializeField] private int MaxForecastPanels = 5;

        /// <summary>
        /// How often to refresh the weather forecast in seconds.
        /// </summary>
        [Tooltip("How often to refresh the weather forecast in seconds")]
        [SerializeField] private float WeatherForecastRefreshRate = 600f;

        /// <summary>
        /// A list of the forecast fields that display the weather forecast.
        /// </summary>
        private readonly List<ForecastField> _forecastFields = new();

        /// <summary>
        /// The cancellation token source for managing the fetch loop.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

#if UNITY_EDITOR
        /// <summary>
        /// For debugging purposes, holds the weather forecast data.
        /// </summary>
        [SerializeField] private List<WeatherForecast> InspectorWeatherForecast;
#endif

        /// <summary>
        /// Initializes the weather panel and starts the fetch loop.
        /// </summary>
        private void Start()
        {
            _forecastFields.Add(ForecastField);
            
            if (PanelData.IsDemoPanel)
            {
                LoadDemoValues();
                return;
            }

            if (!PanelIsReady())
                return;

            StartFetchLoop();
        }

        /// <summary>
        /// Cancels the fetch loop when the panel is disabled.
        /// </summary>
        protected override void OnDisable()
        {
            base.OnDisable();
            CancelFetchLoop();
        }

        /// <summary>
        /// Fetches the weather forecast periodically.
        /// </summary>
        /// <param name="token">The cancellation token to cancel the task.</param>
        private async Task GetHassWeatherForecast(CancellationToken token)
        {
            if (!PanelIsReady())
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
            UpdateForecastFields(forecastResponse);
        }

        /// <summary>
        /// Populates the weather panel with predefined demo values.
        /// Sets static weather information including state, temperature, and icon.
        /// Activates and updates multiple forecast fields with randomized weather predictions for demonstration purposes.
        /// </summary>
        private void LoadDemoValues()
        {
            StateText.text = "Cloudy";
            Temperature.text = "21 °C";
            Icon.text = MaterialDesignIcons.GetWeatherIcon("cloudy");
            
            ActivateFields(5);
            for (int i = 0; i < 5; i++)
            {
                _forecastFields[i].UpdateForecast(new WeatherForecast
                {
                    condition = "sunny",
                    datetime = DateTime.Now.AddDays(i).ToString(CultureInfo.InvariantCulture),
                    temperature = Random.Range(20, 30),
                    templow = Random.Range(10, 20),
                });
            }
        }

        /// <summary>
        /// Updates the current weather state.
        /// </summary>
        private void UpdateCurrentWeather()
        {
            if (PanelData.IsDemoPanel)
                return;
            StateText.text = StringManipulation.CapitalizeFirstLetter(HassState.state);
            Temperature.text = $"{HassState.attributes.temperature} {HassState.attributes.temperature_unit}";
            Icon.text = MaterialDesignIcons.GetWeatherIcon(HassState.state);
        }

        /// <summary>
        /// Updates the forecast fields with the weather forecast data.
        /// </summary>
        /// <param name="forecastResponse">The forecast response from Home Assistant.</param>
        private void UpdateForecastFields(string forecastResponse)
        {
            List<WeatherForecast> weatherForecast = HassStates.ConvertHassWeatherResponse(forecastResponse);

#if UNITY_EDITOR
            // For debugging purposes
            InspectorWeatherForecast = weatherForecast;
#endif

            if (weatherForecast == null)
                return;

            int numberOfForecasts = Math.Min(MaxForecastPanels, weatherForecast.Count);

            ActivateFields(numberOfForecasts);

            for (int i = 0; i < numberOfForecasts; i++)
            {
                _forecastFields[i].UpdateForecast(weatherForecast[i]);
            }
        }

        /// <summary>
        /// Activates or deactivates forecast panels based on the number of forecasts.
        /// </summary>
        /// <param name="numberOfForecasts">The number of forecasts to display.</param>
        private void ActivateFields(int numberOfForecasts)
        {
            while (_forecastFields.Count < numberOfForecasts)
            {
                ForecastField newField = Instantiate(ForecastField, ForecastField.transform.parent);
                _forecastFields.Add(newField);
            }

            for (int i = 0; i < _forecastFields.Count; i++)
            {
                _forecastFields[i].gameObject.SetActive(i < numberOfForecasts);
            }
        }

        /// <summary>
        /// Updates the panel when the panel state changes.
        /// </summary>
        protected override void UpdatePanel()
        {
            base.UpdatePanel();

            if (!PanelIsReady())
                return;

            UpdateCurrentWeather();

            CancelFetchLoop();
            StartFetchLoop();
        }

        /// <summary>
        /// Starts the fetch loop to periodically fetch weather forecasts.
        /// </summary>
        private void StartFetchLoop()
        {
            if (PanelData.IsDemoPanel)
                return;
            _cancellationTokenSource = new CancellationTokenSource();
            _ = GetHassWeatherForecast(_cancellationTokenSource.Token);
        }

        /// <summary>
        /// Cancels the fetch loop.
        /// </summary>
        private void CancelFetchLoop()
        {
            _cancellationTokenSource?.Cancel(); // Cancel any existing token
            _cancellationTokenSource?.Dispose(); // Dispose of the old CTS
        }
    }
}
