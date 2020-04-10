using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem.ViewModelCollection;

namespace KingdomColor.patches
{

    [HarmonyPatch(typeof(BannerEditorVM), "SetClanRelatedRules")]
    class BannerEditorVM_SetClanRelatedRules
    {
        static void Prefix(BannerEditorVM __instance, ref bool canChangeBackgroundColor)
        {
            canChangeBackgroundColor = true;
        }
    }
}
