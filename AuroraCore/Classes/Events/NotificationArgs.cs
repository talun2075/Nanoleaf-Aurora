using System;

namespace AuroraCore.Classes.Events
{
    public class NotificationArgs : EventArgs
    {
        public Notification Notification { get; }

        public NotificationArgs(Notification notification)
        {
            Notification = notification;
        }
    }
}