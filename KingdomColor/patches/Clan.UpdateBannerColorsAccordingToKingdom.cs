using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace KingdomColor.patches
{
    [HarmonyPatch(typeof(Clan), "UpdateBannerColorsAccordingToKingdom")]
    class Clan_UpdateBannerColorsAccordingToKingdom
    {
        static bool Prefix(Clan __instance)
        {
            var clan = __instance;
            if (Settings.Instance != null)
            {
                if (!KingdomColorModule.ShouldReplaceClanColor(clan))
                    return false;
            }
            return true;
        }

        // If FollowKingdomColors is true, we want ruler banner colors to change as well
        static void Postfix(Clan __instance)
        {
            var clan = __instance;
            if (clan.Kingdom?.RulingClan == clan &&
                Settings.Instance.UseClanBannerOverrides &&
                Settings.Instance.GetClanBannerOverride(clan)?.FollowKingdomColors == true)
            {
                clan.Banner?.ChangePrimaryColor(clan.Kingdom.PrimaryBannerColor);
                clan.Banner?.ChangeIconColors(clan.Kingdom.SecondaryBannerColor);
            }
        }
    }
}
