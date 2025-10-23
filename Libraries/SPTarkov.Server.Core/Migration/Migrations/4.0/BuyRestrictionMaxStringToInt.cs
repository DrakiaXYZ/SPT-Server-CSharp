using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using SPTarkov.DI.Annotations;

namespace SPTarkov.Server.Core.Migration.Migrations;

[Injectable]
public class BuyRestrictionMaxStringToInt : AbstractProfileMigration
{
    public override string FromVersion
    {
        get { return "~4.0"; }
    }

    public override string ToVersion
    {
        get { return "~4.0"; }
    }

    public override string MigrationName
    {
        get { return "BuyRestrictionMaxStringToInt400"; }
    }

    public override IEnumerable<Type> PrerequisiteMigrations
    {
        get { return [typeof(ThreeElevenToFourZero)]; }
    }

    public override bool CanMigrate(JsonObject profile, IEnumerable<IProfileMigration> previouslyRanMigrations)
    {
        if (profile?["characters"]?["pmc"]?["Inventory"]?["items"] is JsonArray items)
        {
            foreach (var itemNode in items)
            {
                if (itemNode is not JsonObject itemObj)
                {
                    continue;
                }

                if (itemObj["upd"] is JsonObject updObj)
                {
                    if (updObj.TryGetPropertyValue("BuyRestrictionMax", out var buyRestrictionMaxNode))
                    {
                        if (buyRestrictionMaxNode is JsonValue value && value.TryGetValue(out string? _))
                        {
                            return true;
                        }
                    }
                }
            }
        }

        return false;
    }

    public override JsonObject? Migrate(JsonObject profile)
    {
        if (profile["characters"]?["pmc"]?["Inventory"]?["items"] is JsonArray items)
        {
            foreach (var itemNode in items)
            {
                if (itemNode is not JsonObject itemObj)
                {
                    continue;
                }

                if (itemObj["upd"] is JsonObject updObj && updObj.TryGetPropertyValue("BuyRestrictionMax", out var buyRestrictionMaxNode))
                {
                    if (buyRestrictionMaxNode is JsonValue value && value.TryGetValue(out string? strValue))
                    {
                        if (int.TryParse(strValue, out var intValue))
                        {
                            updObj["BuyRestrictionMax"] = intValue;
                        }
                        else
                        {
                            updObj.Remove("BuyRestrictionMax");
                        }
                    }
                }
            }
        }

        return base.Migrate(profile);
    }
}
