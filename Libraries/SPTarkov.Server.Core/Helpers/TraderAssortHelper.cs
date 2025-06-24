using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Utils;
using SPTarkov.Server.Core.Utils.Cloners;
using LogLevel = SPTarkov.Server.Core.Models.Spt.Logging.LogLevel;

namespace SPTarkov.Server.Core.Helpers;

[Injectable(InjectionType.Singleton)]
public class TraderAssortHelper(
    ISptLogger<TraderAssortHelper> _logger,
    TimeUtil _timeUtil,
    DatabaseService _databaseService,
    ProfileHelper _profileHelper,
    AssortHelper _assortHelper,
    TraderPurchasePersisterService _traderPurchasePersisterService,
    TraderHelper _traderHelper,
    FenceService _fenceService,
    ICloner _cloner
)
{
    private Dictionary<string, Dictionary<string, string>>? _mergedQuestAssorts;
    protected virtual Dictionary<string, Dictionary<string, string>> MergedQuestAssorts
    {
        get { return _mergedQuestAssorts ??= HydrateMergedQuestAssorts(); }
    }

    /// <summary>
    ///     Get a traders assorts
    ///     Can be used for returning ragfair / fence assorts
    ///     Filter out assorts not unlocked due to level OR quest completion
    /// </summary>
    /// <param name="sessionId">session id</param>
    /// <param name="traderId">traders id</param>
    /// <param name="showLockedAssorts">Should assorts player hasn't unlocked be returned - default false</param>
    /// <returns>a traders' assorts</returns>
    public TraderAssort GetAssort(string sessionId, string traderId, bool showLockedAssorts = false)
    {
        var traderClone = _cloner.Clone(_databaseService.GetTrader(traderId));
        var fullProfile = _profileHelper.GetFullProfile(sessionId);
        var pmcProfile = fullProfile?.CharacterData?.PmcData;

        if (traderId == Traders.FENCE)
        {
            return _fenceService.GetFenceAssorts(pmcProfile);
        }

        // Strip assorts player should not see yet
        if (!showLockedAssorts)
        {
            traderClone.Assort = _assortHelper.StripLockedLoyaltyAssort(
                pmcProfile,
                traderId,
                traderClone.Assort
            );
        }

        ResetBuyRestrictionCurrentValue(traderClone.Assort.Items);

        // Append nextResupply value to assorts so client knows when refresh is occuring
        traderClone.Assort.NextResupply = traderClone.Base.NextResupply;

        // Adjust displayed assort counts based on values stored in profile
        var assortPurchasesfromTrader = _traderPurchasePersisterService.GetProfileTraderPurchases(
            sessionId,
            traderId
        );

        foreach (var assortId in assortPurchasesfromTrader ?? [])
        {
            // Find assort we want to update current buy count of
            var assortToAdjust = traderClone.Assort.Items.FirstOrDefault(x => x.Id == assortId.Key);
            if (assortToAdjust is null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Cannot find trader: {traderClone.Base.Nickname} assort: {assortId} to adjust BuyRestrictionCurrent value, skipping"
                    );
                }

                continue;
            }

            if (assortToAdjust.Upd is null)
            {
                if (_logger.IsLogEnabled(LogLevel.Debug))
                {
                    _logger.Debug(
                        $"Unable to adjust assort {assortToAdjust.Id} item: {assortToAdjust.Template} BuyRestrictionCurrent value, assort has a null upd object"
                    );
                }

                continue;
            }

            assortToAdjust.Upd.BuyRestrictionCurrent = (int)(
                assortPurchasesfromTrader[assortId.Key].PurchaseCount ?? 0
            );
        }

        traderClone.Assort = _assortHelper.StripLockedQuestAssort(
            pmcProfile,
            traderId,
            traderClone.Assort,
            MergedQuestAssorts,
            showLockedAssorts
        );

        // Filter out root assorts that are blacklisted for this profile
        if (fullProfile.SptData.BlacklistedItemTemplates?.Count > 0)
        {
            RemoveItemsFromAssort(traderClone.Assort, fullProfile.SptData.BlacklistedItemTemplates);
        }

        return traderClone.Assort;
    }

    /// <summary>
    ///     Given the blacklist provided, remove root items from assort
    /// </summary>
    /// <param name="assortToFilter">Trader assort to modify</param>
    /// <param name="itemsTplsToRemove">Item TPLs the assort should not have</param>
    protected void RemoveItemsFromAssort(
        TraderAssort assortToFilter,
        HashSet<string> itemsTplsToRemove
    )
    {
        assortToFilter.Items = assortToFilter
            .Items.Where(item =>
                item.ParentId == "hideout" && itemsTplsToRemove.Contains(item.Template)
            )
            .ToList();
    }

    /// <summary>
    ///     Reset every traders root item `BuyRestrictionCurrent` property to 0
    /// </summary>
    /// <param name="assortItems">Items to adjust</param>
    protected void ResetBuyRestrictionCurrentValue(List<Item> assortItems)
    {
        // iterate over root items
        foreach (var assort in assortItems.Where(item => item.SlotId == "hideout"))
        {
            // no value to adjust
            if (assort.Upd.BuyRestrictionCurrent is null)
            {
                continue;
            }

            assort.Upd.BuyRestrictionCurrent = 0;
        }
    }

    /// <summary>
    /// Create a dictionary keyed by quest status (started/success) with every assortId to QuestId from every trader
    /// </summary>
    /// <returns>Dictionary</returns>
    protected Dictionary<string, Dictionary<string, string>> HydrateMergedQuestAssorts()
    {
        var result = new Dictionary<string, Dictionary<string, string>>();

        // Loop every trader
        var traders = _databaseService.GetTraders();
        foreach (var (_, trader) in traders)
        {
            if (trader?.QuestAssort is null)
            {
                // No assort to quest mappings, ignore
                continue;
            }

            foreach (var (unlockStatus, assortToQuestDict) in trader.QuestAssort)
            {
                if (!assortToQuestDict.Any())
                {
                    // Empty assort dict, ignore
                    continue;
                }

                // Null guard - ensure Started/Success/fail exists
                result.TryAdd(unlockStatus, new Dictionary<string, string>());

                foreach (var (assortId, questId) in assortToQuestDict)
                {
                    result[unlockStatus][assortId] = questId;
                }
            }
        }

        return result;
    }

    /// <summary>
    ///     Reset a traders assorts and move nextResupply value to future
    ///     Flag trader as needing a flea offer reset to be picked up by flea update() function
    /// </summary>
    /// <param name="trader">trader details to alter</param>
    public void ResetExpiredTrader(Trader trader)
    {
        trader.Assort.Items = GetPristineTraderAssorts(trader.Base.Id);

        // Update resupply value to next timestamp
        trader.Base.NextResupply = (int)_traderHelper.GetNextUpdateTimestamp(trader.Base.Id);

        // Flag a refresh is needed so ragfair update() will pick it up
        trader.Base.RefreshTraderRagfairOffers = true;
    }

    /// <summary>
    ///     Does the supplied trader need its assorts refreshed
    /// </summary>
    /// <param name="traderID">Trader to check</param>
    /// <returns>true they need refreshing</returns>
    public bool TraderAssortsHaveExpired(string traderID)
    {
        var time = _timeUtil.GetTimeStamp();
        var trader = _databaseService.GetTables().Traders[traderID];

        return trader.Base.NextResupply <= time;
    }

    /// <summary>
    ///     Get an array of pristine trader items prior to any alteration by player (as they were on server start)
    /// </summary>
    /// <param name="traderId">trader id</param>
    /// <returns>array of Items</returns>
    protected List<Item> GetPristineTraderAssorts(string traderId)
    {
        return _cloner.Clone(_traderHelper.GetTraderAssortsByTraderId(traderId).Items);
    }
}
