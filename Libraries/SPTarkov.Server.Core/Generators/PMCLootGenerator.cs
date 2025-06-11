using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;

namespace SPTarkov.Server.Core.Generators;

[Injectable]
public class PMCLootGenerator
{
    private readonly ConfigServer _configServer;
    private readonly DatabaseService _databaseService;
    private readonly ItemFilterService _itemFilterService;
    private readonly ItemHelper _itemHelper;
    private readonly ISptLogger<PMCLootGenerator> _logger;
    private readonly PmcConfig _pmcConfig;
    private readonly RagfairPriceService _ragfairPriceService;
    private readonly SeasonalEventService _seasonalEventService;
    private readonly WeightedRandomHelper _weightedRandomHelper;

    private Dictionary<string, double>? _backpackLootPool;
    private Dictionary<string, double>? _pocketLootPool;
    private Dictionary<string, double>? _vestLootPool;

    protected readonly Lock BackpackLock = new();
    protected readonly Lock PocketLock = new();
    protected readonly Lock VestLock = new();

    public PMCLootGenerator(
        ISptLogger<PMCLootGenerator> logger,
        DatabaseService databaseService,
        ItemHelper itemHelper,
        ItemFilterService itemFilterService,
        RagfairPriceService ragfairPriceService,
        SeasonalEventService seasonalEventService,
        WeightedRandomHelper weightedRandomHelper,
        ConfigServer configServer
    )
    {
        _logger = logger;
        _databaseService = databaseService;
        _itemHelper = itemHelper;
        _itemFilterService = itemFilterService;
        _ragfairPriceService = ragfairPriceService;
        _seasonalEventService = seasonalEventService;
        _weightedRandomHelper = weightedRandomHelper;
        _configServer = configServer;

        _pmcConfig = _configServer.GetConfig<PmcConfig>();
    }

    /// <summary>
    ///     Create a List of loot items a PMC can have in their pockets
    /// </summary>
    /// <param name="pmcRole">Role of PMC having loot generated (bear or usec)</param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCPocketLootPool(string pmcRole)
    {
        lock (PocketLock)
        {
            // Hydrate loot dictionary if empty
            if (_pocketLootPool is not null)
            {
                return _pocketLootPool;
            }

            _pocketLootPool = new Dictionary<string, double>();
            var items = _databaseService.GetItems();
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(pmcRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items
                    .Pockets;

            var allowedItemTypeWhitelist = _pmcConfig.PocketLoot.Whitelist;

            var blacklist = GetContainerLootBlacklist();

            var itemsToAdd = items.Where(item =>
                allowedItemTypeWhitelist.Contains(item.Value.Parent) &&
                _itemHelper.IsValidItem(item.Value.Id) &&
                !blacklist.Contains(item.Value.Id) &&
                !blacklist.Contains(item.Value.Parent) &&
                ItemFitsInto1By2Slot(item.Value)
            ).Select(x => x.Key);

            foreach (var tpl in itemsToAdd)
                // If pmc has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var priceOverride))
                {
                    _pocketLootPool.TryAdd(tpl, priceOverride);
                }
                else
                {
                    // Set price of item as its weight
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _pocketLootPool[tpl] = price ?? 0;
                }
            }

            var highestPrice = _pocketLootPool.Max(price => price.Value);
            foreach (var (key, _) in _pocketLootPool)
                // Invert price so cheapest has a larger weight
                // Times by highest price so most expensive item has weight of 1
            {
                _pocketLootPool[key] = Math.Round(1 / _pocketLootPool[key] * highestPrice);
            }

            _weightedRandomHelper.ReduceWeightValues(_pocketLootPool);

            return _pocketLootPool;

        }
    }

    /// <summary>
    /// Get a generic all-container blacklist
    /// </summary>
    /// <returns>Hashset of blacklisted items</returns>
    protected HashSet<string> GetContainerLootBlacklist()
    {
        var blacklist = new HashSet<string>();
        blacklist.UnionWith(_pmcConfig.PocketLoot.Blacklist);
        blacklist.UnionWith(_pmcConfig.GlobalLootBlacklist);
        blacklist.UnionWith(_itemFilterService.GetBlacklistedItems());
        blacklist.UnionWith(_itemFilterService.GetBlacklistedLootableItems());
        blacklist.UnionWith(_seasonalEventService.GetInactiveSeasonalEventItems());

        return blacklist;
    }

    /// <summary>
    ///     Create a dictionary of loot items a PMC can have in their vests with a corresponding weight of being picked to spawn
    /// </summary>
    /// <param name="pmcRole">Role of PMC having loot generated (bear or usec)</param>
    /// <returns>Dictionary item template ids and a weighted chance of being picked</returns>
    public Dictionary<string, double> GeneratePMCVestLootPool(string pmcRole)
    {
        lock (VestLock)
        {
            // Hydrate loot dictionary if empty
            if (_vestLootPool is not null)
            {
                return _vestLootPool;
            }

            // Create dictionary to hold vest loot
            _vestLootPool = new Dictionary<string, double>();

            // Get all items from database
            var items = _databaseService.GetItems();

            // Grab price overrides if they exist for the pmcRole passed in
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(pmcRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items
                    .TacticalVest;

            var blacklist = GetContainerLootBlacklist();
            blacklist.UnionWith(_pmcConfig.VestLoot.Blacklist); // Add vest-specific blacklist

            var itemTplsToAdd = items.Where(item =>
                _pmcConfig.VestLoot.Whitelist.Contains(item.Value.Parent) && // A whitelist of item types the PMC is allowed to have
                !blacklist.Contains(item.Value.Id) &&
                !blacklist.Contains(item.Value.Parent) &&
                _itemHelper.IsValidItem(item.Value.Id) &&
                ItemFitsInto2By2Slot(item.Value)
            ).Select(x => x.Key);

            foreach (var tpl in itemTplsToAdd)
                // If PMC has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var overridePrice))
                {
                    // There's a price override for this item, use override instead of default price
                    _vestLootPool.TryAdd(tpl, overridePrice);
                }
                else
                {
                    // Store items price so we can turn it into a weighting later
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _vestLootPool[tpl] = price ?? 0;
                }
            }

            // Find the highest priced item added to vest pool
            var highestPrice = _vestLootPool.Max(price => price.Value);
            foreach (var (key, _) in _vestLootPool)
                // Invert price so cheapest has a larger weight, giving us a weighting of low-priced items being more common
                // Times by highest price so most expensive item has weight of 1
            {
                _vestLootPool[key] = Math.Round(1 / _vestLootPool[key] * highestPrice);
            }

            // Find the greatest common divisor between all the prices and apply it to reduce the values for better readability of weights
            _weightedRandomHelper.ReduceWeightValues(_vestLootPool);

            return _vestLootPool;
        }
    }

    /// <summary>
    ///     Check if item has a width/height that lets it fit into a 2x2 slot
    ///     1x1 / 1x2 / 2x1 / 2x2
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto2By2Slot(TemplateItem item)
    {
        return item.Properties.Width <= 2 && item.Properties.Height <= 2;
    }

    /// <summary>
    ///     Check if item has a width/height that lets it fit into a 1x2 slot
    ///     1x1 / 1x2 / 2x1
    /// </summary>
    /// <param name="item">Item to check size of</param>
    /// <returns>true if it fits</returns>
    protected bool ItemFitsInto1By2Slot(TemplateItem item)
    {
        return $"{item.Properties.Width}x{item.Properties.Height}" switch
        {
            "1x1" or "1x2" or "2x1" => true,
            _ => false
        };
    }

    /// <summary>
    ///     Create a List of loot items a PMC can have in their backpack
    /// </summary>
    /// <param name="botRole">Role of PMC having loot generated (bear or usec)</param>
    /// <returns>Dictionary of string and number</returns>
    public Dictionary<string, double> GeneratePMCBackpackLootPool(string botRole)
    {
        lock (BackpackLock)
        {
            // Hydrate loot dictionary if empty
            if (_backpackLootPool is not null)
            {
                return _backpackLootPool;
            }

            _backpackLootPool = new Dictionary<string, double>();
            var items = _databaseService.GetItems();
            var pmcPriceOverrides =
                _databaseService.GetBots().Types[string.Equals(botRole, "pmcbear", StringComparison.OrdinalIgnoreCase) ? "bear" : "usec"].BotInventory.Items
                    .Backpack;

            var allowedItemTypeWhitelist = _pmcConfig.BackpackLoot.Whitelist;

            var blacklist = GetContainerLootBlacklist();
            blacklist.UnionWith(_pmcConfig.BackpackLoot.Blacklist); // Add backpack-specific blacklist

            var itemsToAdd = items.Where(item =>
                allowedItemTypeWhitelist.Contains(item.Value.Parent) &&
                _itemHelper.IsValidItem(item.Value.Id) &&
                !blacklist.Contains(item.Value.Id) &&
                !blacklist.Contains(item.Value.Parent)
            ).Select(x => x.Key);

            foreach (var tpl in itemsToAdd)
                // If pmc has price override, use that. Otherwise, use flea price
            {
                if (pmcPriceOverrides.TryGetValue(tpl, out var priceOverride))
                {
                    _backpackLootPool.TryAdd(tpl, priceOverride);
                }
                else
                {
                    // Set price of item as its weight
                    var price = _ragfairPriceService.GetDynamicItemPrice(tpl, Money.ROUBLES);
                    _backpackLootPool[tpl] = price ?? 0;
                }
            }

            var highestPrice = _backpackLootPool.Max(price => price.Value);
            foreach (var (key, _) in _backpackLootPool)
                // Invert price so cheapest has a larger weight
                // Times by highest price so most expensive item has weight of 1
            {
                _backpackLootPool[key] = Math.Round(1 / _backpackLootPool[key] * highestPrice);
            }

            _weightedRandomHelper.ReduceWeightValues(_backpackLootPool);

            return _backpackLootPool;
        }
    }
}
