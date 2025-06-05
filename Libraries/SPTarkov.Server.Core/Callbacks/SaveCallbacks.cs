using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnLoadOrder.SaveCallbacks)]
public class SaveCallbacks(
    SaveServer _saveServer,
    ConfigServer _configServer,
    BackupService _backupService,
    TimeUtil _timeUtil
)
    : IOnLoad, IOnUpdate
{
    private readonly CoreConfig _coreConfig = _configServer.GetConfig<CoreConfig>();
    private long _lastRunOnUpdateTimestamp = long.MaxValue;

    public async Task OnLoad()
    {
        _backupService.StartBackupSystem();
        _saveServer.Load();
    }

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (_timeUtil.GetTimeStamp() <= _lastRunOnUpdateTimestamp + _coreConfig.ProfileSaveIntervalInSeconds)
        {
            // Not enough time has passed since last run, exit early
            return Task.FromResult(false);
        }

        _saveServer.Save();

        // Store last completion time for later use
        _lastRunOnUpdateTimestamp = _timeUtil.GetTimeStamp();

        return Task.FromResult(false);
    }
}
