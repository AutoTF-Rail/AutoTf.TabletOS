namespace AutoTf.TabletOS.Models.Interfaces;

public interface INetworkService
{
    public static abstract bool IsInternetAvailable();

    public void ShutdownConnection();

    public string? EstablishConnection(string name, bool isTrain);

    public void ScanForMesh();
}