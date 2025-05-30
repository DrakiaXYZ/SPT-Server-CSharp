namespace SPTarkov.Server.Core.DI;

public interface ISerializer
{
    public Task Serialize(string sessionID, HttpRequest req, HttpResponse resp, object? body);
    public bool CanHandle(string route);
}
