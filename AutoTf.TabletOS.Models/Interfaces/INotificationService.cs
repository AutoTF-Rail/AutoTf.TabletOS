using System.Collections.ObjectModel;

namespace AutoTf.TabletOS.Models.Interfaces;

public interface INotificationService
{
    public ObservableCollection<Notification> Notifications { get; }
    
    public void Success(string message);
    public void Info(string message);
    public void Warn(string message);
    public void Error(string message);
    public void Remove(Notification notification);
}