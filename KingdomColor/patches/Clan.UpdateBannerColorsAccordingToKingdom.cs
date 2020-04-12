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
        static bool Prefix(Clan __instance, ref ClanBanner __state)
        {
            var clan = __instance;
            if (Settings.Instance != null)
            {
                if (clan == Clan.PlayerClan)
                {
                    if (!Settings.Instance.PlayerClanBannerFollowsKingdom)
                        return false;
                }
                if (Settings.Instance.UseClanBannerOverrides)
                {
                    var info = Settings.Instance.GetClanBannerOverride(clan);
                    __state = info;
                    if (info != null)
                    {
                        if (!info.FollowKingdomColors)
                            return false;
                    }
                }
            }
            return true;
        }

        // If FollowKingdomColors is true, we want ruler banner colors to change as well
        static void Postfix(Clan __instance, ref ClanBanner __state)
        {
            var clan = __instance;
            var info = __state;
            if (info != null && clan?.Kingdom?.RulingClan == clan && info.FollowKingdomColors)
            {
                clan.Banner?.ChangePrimaryColor(clan.Kingdom.PrimaryBannerColor);
                clan.Banner?.ChangeIconColors(clan.Kingdom.SecondaryBannerColor);
            }
        }
    }
}
