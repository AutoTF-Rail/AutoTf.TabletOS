namespace AutoTf.TabletOS.Models;

public class YubiKeySession
{
    public string Code { get; set; } = string.Empty;
    public int SerialNumber { get; set; } = -1;
    public DateTime Time { get; set; } = DateTime.MinValue;

    public void Empty()
    {
        Code = string.Empty;
        SerialNumber = -1;
        Time = DateTime.MinValue;
    }
}