using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Enums;

[EftEnumConverter]
public enum RewardType
{
    Experience,
    Skill,
    Item,
    TraderStanding,
    TraderUnlock,
    Location,
    Counter,
    AssortmentUnlock,
    ProductionScheme,
    TraderStandingReset,
    TraderStandingRestore,
    StashRows,
    Achievement,
    Pockets,
    Quest,
    CustomizationOffer,
    ExtraDailyQuest,
    CustomizationDirect,
    WebPromoCode,
    NotificationPopup,
    Customization = 116,
    BattlePassExperience,
    BattlePassCurrency,
    ArenaArmoryItem = 100,
}
