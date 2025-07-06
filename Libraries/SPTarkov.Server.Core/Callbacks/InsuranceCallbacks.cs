﻿using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Controllers;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Insurance;
using SPTarkov.Server.Core.Models.Eft.ItemEvent;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Utils;

namespace SPTarkov.Server.Core.Callbacks;

[Injectable(TypePriority = OnUpdateOrder.InsuranceCallbacks)]
public class InsuranceCallbacks(
    InsuranceController insuranceController,
    HttpResponseUtil httpResponseUtil,
    ConfigServer configServer
) : IOnUpdate
{
    private readonly InsuranceConfig _insuranceConfig = configServer.GetConfig<InsuranceConfig>();

    public Task<bool> OnUpdate(long secondsSinceLastRun)
    {
        if (secondsSinceLastRun < _insuranceConfig.RunIntervalSeconds)
        {
            return Task.FromResult(false);
        }

        insuranceController.ProcessReturn();

        return Task.FromResult(true);
    }

    /// <summary>
    ///     Handle client/insurance/items/list/cost
    /// </summary>
    /// <param name="url"></param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ValueTask<string> GetInsuranceCost(
        string url,
        GetInsuranceCostRequestData info,
        MongoId sessionID
    )
    {
        return new ValueTask<string>(
            httpResponseUtil.GetBody(insuranceController.Cost(info, sessionID))
        );
    }

    /// <summary>
    ///     Handle Insure event
    /// </summary>
    /// <param name="pmcData">Players PMC profile</param>
    /// <param name="info"></param>
    /// <param name="sessionID">Session/player id</param>
    /// <returns></returns>
    public ItemEventRouterResponse Insure(
        PmcData pmcData,
        InsureRequestData info,
        MongoId sessionID
    )
    {
        return insuranceController.Insure(pmcData, info, sessionID);
    }
}
