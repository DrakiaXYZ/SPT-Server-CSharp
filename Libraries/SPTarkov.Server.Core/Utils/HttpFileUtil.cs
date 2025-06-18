using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace SPTarkov.Server.Core.Utils;

[Injectable]
public class HttpFileUtil(HttpServerHelper httpServerHelper)
{
    protected HttpServerHelper _httpServerHelper = httpServerHelper;

    public async Task SendFile(HttpResponse resp, string filePath)
    {
        var pathSlice = filePath.Split("/");
        var mimePath = _httpServerHelper.GetMimeText(pathSlice[^1].Split(".")[^1]);
        var type = string.IsNullOrWhiteSpace(mimePath)
            ? _httpServerHelper.GetMimeText("txt")
            : mimePath;
        resp.Headers.Append("Content-Type", type);
        await resp.SendFileAsync(filePath, CancellationToken.None);
    }
}
