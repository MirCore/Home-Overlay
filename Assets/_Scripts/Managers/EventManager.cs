using System;

namespace Managers
{
    public static class EventManager
    {
        /// <summary>
        /// Event triggered when the connection to Home Assistant is tested.
        /// The parameter is the status of the connection test.
        /// </summary>
        public static event Action<int, Uri> OnConnectionTested;

        /// <summary>
        /// Invokes the <see cref="OnConnectionTested"/> event.
        /// </summary>
        /// <param name="status">The status of the connection test.</param>
        /// <param name="uri">The tested Uri</param>
        public static void InvokeOnConnectionTested(int status, Uri uri = null) => OnConnectionTested?.Invoke(status, uri);

        
        /// <summary>
        /// Event triggered when new states are received from Home Assistant.
        /// </summary>
        public static event Action OnHassStatesChanged;
        /// <summary>
        /// Invokes the <see cref="OnHassStatesChanged"/> event.
        /// </summary>
        public static void InvokeOnHassStatesChanged() => OnHassStatesChanged?.Invoke();
        
        
        /// <summary>
        /// Event triggered when the App State was initially loaded.
        /// </summary>
        public static event Action OnAppStateLoaded;
        /// <summary>
        /// Invokes the <see cref="OnAppStateLoaded"/> event.
        /// </summary>
        public static void InvokeOnAppStateLoaded() => OnAppStateLoaded?.Invoke();
    }
}