using System;

namespace Managers
{
    public static class EventManager
    {
        public static event Action<string> OnConnectionTested;
        public static void InvokeOnConnectionTested(string status) => OnConnectionTested?.Invoke(status);
    }
}