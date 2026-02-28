using System;

public static class NotificationBus
{
    public static event Action<string> OnStatusMessage;

    public static void PostMessage(string message)
    {
        OnStatusMessage?.Invoke(message);
    }
}