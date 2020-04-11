using HarmonyLib;
using SandBox.GauntletUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace KingdomColor.patches
{
    [HarmonyPatch(typeof(MBBannerEditorGauntletScreen), "OnDone")]
    class MBBannerEditorGauntletScreen_OnDone
    {
        // Prefix to get in before the GameStateManager.PopState
        static void Prefix(MBBannerEditorGauntletScreen __instance)
        {
            try
            {
                var _bannerEditorLayer = Traverse.Create(__instance).Field<BannerEditorView>("_bannerEditorLayer").Value;
                var playerClan = Traverse.Create(__instance).Field<Clan>("_clan").Value;
                var color1 = _bannerEditorLayer.DataSource.BannerVM.GetPrimaryColor();
                var color2 = _bannerEditorLayer.DataSource.BannerVM.GetSigilColor();
                KingdomColorModule.Instance.SetClanKingdomColors(playerClan, color1, color2);
            }
            catch (Exception ex)
            {
                Log.write("Error applying new banner colors");
                InformationManager.DisplayMessage(new InformationMessage("Error applying clan colors", new Color(1f, 0, 0)));
                Log.write(ex);
            }
        }
    }
}
