﻿using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable]
public class ClientLogCallbacks(
    HttpResponseUtil httpResponseUtil,
    ClientLogController clientLogController,
    ConfigServer configServer,
    ServerLocalisationService serverLocalisationService,
    IReadOnlyList<SptMod> loadedMods
)
{
    /// <summary>
    ///     Handle /singleplayer/log
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ClientLog(string url, ClientLogRequest request, MongoId sessionID)
    {
        clientLogController.ClientLog(request);
        return new ValueTask<string>(httpResponseUtil.NullResponse());
    }

    /// <summary>
    ///     Handle /singleplayer/release
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> ReleaseNotes()
    {
        var data = configServer.GetConfig<CoreConfig>().Release;

        data.BetaDisclaimerText = ProgramStatics.MODS()
            ? serverLocalisationService.GetText("release-beta-disclaimer-mods-enabled")
            : serverLocalisationService.GetText("release-beta-disclaimer");

        data.BetaDisclaimerAcceptText = serverLocalisationService.GetText(
            "release-beta-disclaimer-accept"
        );
        data.ServerModsLoadedText = serverLocalisationService.GetText("release-server-mods-loaded");
        data.ServerModsLoadedDebugText = serverLocalisationService.GetText(
            "release-server-mods-debug-message"
        );
        data.ClientModsLoadedText = serverLocalisationService.GetText("release-plugins-loaded");
        data.ClientModsLoadedDebugText = serverLocalisationService.GetText(
            "release-plugins-loaded-debug-message"
        );
        data.IllegalPluginsLoadedText = serverLocalisationService.GetText(
            "release-illegal-plugins-loaded"
        );
        data.IllegalPluginsExceptionText = serverLocalisationService.GetText(
            "release-illegal-plugins-exception"
        );
        data.ReleaseSummaryText = serverLocalisationService.GetText("release-summary");
        data.IsBeta =
            ProgramStatics.ENTRY_TYPE() is EntryType.BLEEDING_EDGE or EntryType.BLEEDING_EDGE_MODS;
        data.IsModdable = ProgramStatics.MODS();
        data.IsModded = loadedMods.Count > 0;

        return new ValueTask<string>(httpResponseUtil.NoBody(data));
    }

    /// <summary>
    ///     Handle /singleplayer/enableBSGlogging
    /// </summary>
    /// <returns></returns>
    public ValueTask<string> BsgLogging()
    {
        var data = configServer.GetConfig<CoreConfig>().BsgLogging;
        return new ValueTask<string>(httpResponseUtil.NoBody(data));
    }
}
