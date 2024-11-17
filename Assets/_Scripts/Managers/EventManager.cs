using System;

namespace Managers
{
    public static class EventManager
    {
        /// <summary>
        /// Event triggered when the connection to Home Assistant is tested.
        /// The parameter is the status of the connection test.
        /// </summary>
        public static event Action<string> OnConnectionTested;
        /// <summary>
        /// Invokes the OnConnectionTested event.
        /// </summary>
        /// <param name="status">The status of the connection test.</param>
        public static void InvokeOnConnectionTested(string status) => OnConnectionTested?.Invoke(status);

        
        /// <summary>
        /// Event triggered when the HassStates are changed.
        /// </summary>
        public static event Action OnHassStatesChanged;
        /// <summary>
        /// Invokes the OnHassStatesChanged event.
        /// </summary>
        public static void InvokeOnHassStatesChanged() => OnHassStatesChanged?.Invoke();
    }
}