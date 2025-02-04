using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace Entity
{
    public class EntityCalendar : Entity
    {
        [SerializeField] private TMP_Text Month;
        [SerializeField] private TMP_Text Day;
        [SerializeField] private TMP_Text Weekday;
        [SerializeField] private TMP_Text Time;
        [SerializeField] private TMP_Text Event;
        [SerializeField] private TMP_Text Location;
        
        [SerializeField] private List<CalendarEvent> InspectorCalendarEvents;
        
        /// <summary>
        /// How often to refresh the Calendar in seconds
        /// </summary>
        [Tooltip("How often to refresh the Calendar in seconds")]
        [SerializeField] private float CalendarRefreshRate = 600f;
        
        private CancellationTokenSource _cancellationTokenSource;
        
        private void Start()
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _ = GetHassCalendar(_cancellationTokenSource.Token);
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            _cancellationTokenSource?.Cancel();
        }
        

        /// <summary>
        /// Updates the calendar periodically.
        /// </summary>
        /// <param name="token">The cancellation token to cancel the task.</param>
        private async Task GetHassCalendar(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    RestHandler.GetCalendar(HassState.entity_id, UpdateCalendar);
                    await Task.Delay(TimeSpan.FromSeconds(CalendarRefreshRate), token);
                }
                catch (TaskCanceledException)
                {
                    //Debug.Log("Calendar update task was canceled.");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error while updating calendar: {ex.Message}");
                }
            }
        }
        
        private void UpdateCalendar(string calendarResponse)
        {
            UpdateNextEvent(calendarResponse);
        }
        /// <summary>
        /// Updates the calendar panel with the next event.
        /// </summary>
        /// <param name="calendarResponse">The response from Home Assistant containing the calendar events.</param>
        private void UpdateNextEvent(string calendarResponse)
        {
            List<CalendarEvent> events = HassStates.ConvertHassCalendarResponse(calendarResponse);
            InspectorCalendarEvents = events;
            
            // Find the next event by ordering by start time and taking the first event that ends after the current time
            // The events are ordered by start time, so the first event that ends after the current time is the next event
            CalendarEvent nextEvent = events
                .Where(e => e.start.GetDateTime() != null && e.end.GetDateTime() > DateTime.Now)
                .OrderBy(e => e.start.GetDateTime())
                .FirstOrDefault();

            if (nextEvent != null)
            {
                // Set the calendar panel text
                Month.text = nextEvent.start.GetDateTime()?.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
                Day.text = nextEvent.start.GetDateTime()?.Day.ToString();
                Weekday.text = nextEvent.start.GetDateTime()?.ToString("ddd", CultureInfo.InvariantCulture);

                // If the start time is null, it's an all day event, so don't show the time
                Time.text = nextEvent.start.dateTime == null ? "All Day Event" : $"{nextEvent.start.GetDateTime()?.ToShortTimeString()} - {nextEvent.end.GetDateTime()?.ToShortTimeString()}";
                Event.text = nextEvent.summary;
                Location.text = nextEvent.location;
            }
            else
            {
                // Show the current date and time if there are no upcoming events
                Month.text = DateTime.Now.Month.ToString("MMM", CultureInfo.InvariantCulture).ToUpper();
                Day.text = DateTime.Now.Day.ToString();
                Weekday.text = DateTime.Now.ToString("ddd", CultureInfo.InvariantCulture);
                Time.text = "";
                Event.text = "No upcoming events";
            }
        }

        protected override void UpdateEntity()
        {
        }
    }
}
