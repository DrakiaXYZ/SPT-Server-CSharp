using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Callbacks;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Routers.Static;

[Injectable]
public class ModdedTraderCustomizationRouter(JsonUtil jsonUtil, ModdedTraderCustomizationCallbacks moddedTraderCustomizationCallbacks)
    : StaticRouter(
        jsonUtil,
        [
            new RouteAction<EmptyRequestData>(
                "/singleplayer/moddedTraders",
                async (url, info, sessionID, output) =>
                    await moddedTraderCustomizationCallbacks.GetCustomizationTraders(url, info, sessionID)
            )
        ]
    ) { }
