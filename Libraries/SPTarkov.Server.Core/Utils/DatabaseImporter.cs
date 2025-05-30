using System.Diagnostics;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Spt.Server;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Routers;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Utils;

[Injectable(InjectionType.Singleton, TypePriority = OnLoadOrder.Database)]
public class DatabaseImporter(
    ISptLogger<DatabaseImporter> logger,
    FileUtil _fileUtil,
    LocalisationService _localisationService,
    DatabaseServer _databaseServer,
    ImageRouter _imageRouter,
    ImporterUtil _importerUtil
    ) : IOnLoad
{
    private const string _sptDataPath = "./Assets/";
    protected ISptLogger<DatabaseImporter> _logger = logger;

    public async Task OnLoad()
    {
        await HydrateDatabase(_sptDataPath);

        var imageFilePath = $"{_sptDataPath}images/";
        CreateRouteMapping(imageFilePath, "files");
    }

    private void CreateRouteMapping(string directory, string newBasePath)
    {
        var directoryContent = GetAllFilesInDirectory(directory);

        foreach (var fileNameWithPath in directoryContent)
        {
            var fileNameWithNoSPTPath = fileNameWithPath.Replace(directory, "");
            var filePathNoExtension = _fileUtil.StripExtension(fileNameWithNoSPTPath, true);
            if (filePathNoExtension.StartsWith("/") || fileNameWithPath.StartsWith("\\"))
            {
                filePathNoExtension = $"{filePathNoExtension.Substring(1)}";
            }

            var bsgPath = $"/{newBasePath}/{filePathNoExtension}".Replace("\\", "/");
            _imageRouter.AddRoute(bsgPath, fileNameWithPath);
        }
    }

    private List<string> GetAllFilesInDirectory(string directoryPath)
    {
        List<string> result = [];
        result.AddRange(Directory.GetFiles(directoryPath));

        foreach (var subdirectory in Directory.GetDirectories(directoryPath))
        {
            result.AddRange(GetAllFilesInDirectory(subdirectory));
        }

        return result;
    }

    /**
     * Read all json files in database folder and map into a json object
     * @param filepath path to database folder
     */
    protected async Task HydrateDatabase(string filePath)
    {
        _logger.Info(_localisationService.GetText("importing_database"));
        Stopwatch timer = new();
        timer.Start();

        var dataToImport = await _importerUtil.LoadRecursiveAsync<DatabaseTables>(
            $"{filePath}database/"
        );

        // TODO: Fix loading of traders, so their full path is not included as the key

        var tempTraders = new Dictionary<string, Trader>();

        // temp fix for trader keys
        foreach (var trader in dataToImport.Traders)
        {
            // fix string for key
            var tempKey = trader.Key.Split("/").Last();
            tempTraders.Add(tempKey, trader.Value);
        }

        timer.Stop();

        dataToImport.Traders = tempTraders;

        _logger.Info( _localisationService.GetText("importing_database_finish"));
        _logger.Debug($"Database import took {timer.ElapsedMilliseconds}ms");
        _databaseServer.SetTables(dataToImport);
    }
}
