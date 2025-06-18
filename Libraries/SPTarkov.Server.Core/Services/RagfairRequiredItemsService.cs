using System.Collections.Concurrent;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.Helpers;

namespace SPTarkov.Server.Core.Services;

[Injectable(InjectionType.Singleton)]
public class RagfairRequiredItemsService(
    RagfairOfferService ragfairOfferService,
    PaymentHelper paymentHelper
)
{
    /// <summary>
    /// Key = tpl
    /// </summary>
    protected readonly ConcurrentDictionary<string, HashSet<string>> _requiredItemsCache = new();

    /// <summary>
    /// Get the offerId of offers that require the supplied tpl
    /// </summary>
    /// <param name="tpl">Tpl to find offers ids for</param>
    /// <returns></returns>
    public HashSet<string> GetRequiredOffersById(string tpl)
    {
        if (_requiredItemsCache.TryGetValue(tpl, out var offerIds))
        {
            return offerIds;
        }

        return [];
    }

    /// <summary>
    /// Create a cache of requirements to purchase item
    /// </summary>
    public void BuildRequiredItemTable()
    {
        _requiredItemsCache.Clear();
        foreach (var offer in ragfairOfferService.GetOffers())
        foreach (var requirement in offer.Requirements)
        {
            if (paymentHelper.IsMoneyTpl(requirement.Template))
            // This would just be too noisy
            {
                continue;
            }

            // Ensure key is init
            _requiredItemsCache.TryAdd(requirement.Template, []);

            // Add matching offer
            _requiredItemsCache.GetValueOrDefault(requirement.Template)?.Add(offer.Id);
        }
    }
}
