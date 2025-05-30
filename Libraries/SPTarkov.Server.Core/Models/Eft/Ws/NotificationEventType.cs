using SPTarkov.Server.Core.Utils.Json.Converters;

namespace SPTarkov.Server.Core.Models.Eft.Ws;

[EftEnumConverter]
public enum NotificationEventType
{
    AssortmentUnlockRule,
    ExamineItems,
    ExamineAllItems,
    ForceLogout,
    RagfairOfferSold,
    RagfairNewRating,
    RagfairRatingChange,
    new_message,
    ping,
    TraderSalesSum,
    trader_supply,
    TraderStanding,
    UnlockTrader,
    groupMatchRaidSettings,
    groupMatchRaidNotReady,
    groupMatchRaidReady,
    groupMatchInviteAccept,
    groupMatchInviteDecline,
    groupMatchInviteSend,
    groupMatchLeaderChanged,
    groupMatchStartGame,
    groupMatchUserLeave,
    groupMatchWasRemoved,
    groupMatchUserHasBadVersion,
    userConfirmed,
    UserMatched,
    userMatchOver,
    channel_deleted,
    friendListRequestAccept,
    friendListRequestDecline,
    friendListNewRequest,
    youAreRemovedFromFriendList,
    YouWereAddedToIgnoreList,
    youAreRemoveFromIgnoreList,
    ProfileLockTimer,
    StashRows,
    SkillPoints,
    tournamentWarning
}
