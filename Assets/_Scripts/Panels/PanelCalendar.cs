using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace Panels
{
    /// <summary>
    /// A panel that displays calendar events fetched from Home Assistant.
    /// </summary>
    public class PanelCalendar : Panel
    {
        /// <summary>
        /// The text component for displaying the month.
        /// </summary>
        [SerializeField] private TMP_Text Month;

        /// <summary>
        /// The text component for displaying the day.
        /// </summary>
        [SerializeField] private TMP_Text Day;

        /// <summary>
        /// The text component for displaying the weekday.
        /// </summary>
        [SerializeField] private TMP_Text Weekday;

        /// <summary>
        /// The text component for displaying the time.
        /// </summary>
        [SerializeField] private TMP_Text Time;

        /// <summary>
        /// The text component for displaying the event summary.
        /// </summary>
        [SerializeField] private TMP_Text Event;

        /// <summary>
        /// The text component for displaying the event location.
        /// </summary>
        [SerializeField] private TMP_Text Location;

        /// <summary>
        /// How often to refresh the calendar in seconds.
        /// </summary>
        [Tooltip("How often to refresh the calendar in seconds")]
        [SerializeField] private float CalendarRefreshRate = 600f;

        /// <summary>
        /// The cancellation token source for managing the fetch loop.
        /// </summary>
        private CancellationTokenSource _cancellationTokenSource;

#if UNITY_EDITOR
        /// <summary>
        /// For debugging purposes, holds the calendar events data.
        /// </summary>
        [SerializeField] private List<CalendarEvent> InspectorCalendarEvents;
#endif

        private void Start()
        {
            if (!PanelIsReady())
                return;

            StartFetchLoop();
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            CancelFetchLoop();
        }

        /// <summary>
        /// Fetches the calendar events periodically.
        /// </summary>
        /// <param name="token">The cancellation token to cancel the task.</param>
        private async Task GetHassCalendar(CancellationToken token)
        {
            if (HassState == null)
                _cancellationTokenSource.Cancel();

            while (!token.IsCancellationRequested)
            {
                try
                {
                    if (HassState != null)
                        RestHandler.GetCalendar(HassState.entity_id, UpdateCalendar);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while updating calendar: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(CalendarRefreshRate), token);
            }
        }

        /// <summary>
        /// Updates the calendar panel with the next event.
        /// </summary>
        /// <param name="calendarResponse">The response from Home Assistant containing the calendar events.</param>
        private void UpdateCalendar(string calendarResponse)
        {
            List<CalendarEvent> events = HassStates.ConvertHassCalendarResponse(calendarResponse);

#if UNITY_EDITOR
            // For debugging purposes
            InspectorCalendarEvents = events;
#endif

            // Find the next event by ordering by start time and taking the first event that ends after the current time
            CalendarEvent nextEvent = events
                .Where(e => e.start.GetDateTime() != null && e.end.GetDateTime() > DateTime.Now)
                .OrderBy(e => e.start.GetDateTime())
                .FirstOrDefault();

            if (nextEvent == null)
                SetNoEventText();
            else
                SetEventText(nextEvent);
        }

        /// <summary>
        /// Sets the text for when there are no upcoming events.
        /// </summary>
        private void SetNoEventText()
        {
            // Show the current date and time if there are no upcoming events
            Month.text = DateTime.Now.Month.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
            Day.text = DateTime.Now.Day.ToString();
            Weekday.text = DateTime.Now.ToString("ddd", CultureInfo.InvariantCulture);
            Time.text = "";
            Event.text = "No upcoming events";
            Location.text = "";
        }

        /// <summary>
        /// Sets the text for the next upcoming event.
        /// </summary>
        /// <param name="nextEvent">The next calendar event to display.</param>
        private void SetEventText(CalendarEvent nextEvent)
        {
            // Set the calendar panel text
            Month.text = nextEvent.start.GetDateTime()?.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
            Day.text = nextEvent.start.GetDateTime()?.Day.ToString();
            Weekday.text = nextEvent.start.GetDateTime()?.ToString("ddd", CultureInfo.InvariantCulture);

            // If the start time is null, it's an all-day event, so don't show the time
            Time.text = nextEvent.start.dateTime == null
                ? "All Day Event"
                : $"{nextEvent.start.GetDateTime()?.ToShortTimeString()} - {nextEvent.end.GetDateTime()?.ToShortTimeString()}";
            Event.text = nextEvent.summary;
            Location.text = nextEvent.location;
        }

        /// <summary>
        /// Updates the panel when the state changes.
        /// </summary>
        protected override void UpdatePanel()
        {
            base.UpdatePanel();

            if (!PanelIsReady())
                return;

            CancelFetchLoop();
            StartFetchLoop();
        }

        /// <summary>
        /// Starts the fetch loop to periodically fetch calendar events.
        /// </summary>
        private void StartFetchLoop()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _ = GetHassCalendar(_cancellationTokenSource.Token);
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
