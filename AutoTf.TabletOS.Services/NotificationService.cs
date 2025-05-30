using System.Collections.ObjectModel;
using AutoTf.TabletOS.Models;
using AutoTf.TabletOS.Models.Interfaces;
using Avalonia.Media;

namespace AutoTf.TabletOS.Services;

public class NotificationService : INotificationService
{
    public ObservableCollection<Notification> Notifications { get; } = new();

    public void Success(string message) => AddNotification(message, Colors.Green);

    public void Info(string message) => AddNotification(message, Colors.White);

    public void Warn(string message) => AddNotification(message, Colors.Yellow);

    public void Error(string message) => AddNotification(message, Colors.IndianRed);

    private void AddNotification(string message, Color color)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Notifications.Add(new Notification(message, color));
        });
    }

    public void Remove(Notification notification)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            Notifications.Remove(notification);
        });
    }
}