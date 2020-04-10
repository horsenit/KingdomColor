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
                if (playerClan.Kingdom != null && (!Settings.Instance.OnlyPlayerRuledKingdoms || playerClan.Kingdom.RulingClan == playerClan))
                {
                    var kingdom = playerClan.Kingdom;
                    var k = Traverse.Create(kingdom);
                    var color1 = _bannerEditorLayer.DataSource.BannerVM.GetPrimaryColor();
                    var color2 = _bannerEditorLayer.DataSource.BannerVM.GetSigilColor();
                    k.Property<uint>("Color").Value = color1;
                    k.Property<uint>("Color2").Value = color2;
                    k.Property<uint>("PrimaryBannerColor").Value = color1;
                    k.Property<uint>("SecondaryBannerColor").Value = color2;

                    Log.write($"Updating {kingdom.Name}");
                    Log.write($"Ruling clan {kingdom.RulingClan.Name}");
                    foreach (var kingdomClan in kingdom.Clans)
                    {
                        Log.write($"  Updating clan {kingdomClan} colors");
                        // Don't update player clan colours, we just did that you idiot.
                        if (kingdomClan == playerClan)
                        {
                            Log.write($"!!Skipping clan {playerClan}");
                            continue;
                        }
                        else
                        {
                            // var c = Traverse.Create(kingdomClan);
                            // c.Method("UpdateBannerColorsAccordingToKingdom").GetValue();
                            // UpdateBannerColorsAccordingToKingdom does not refresh ruler colors, but we want to change our NPC kings colors so here we are.
                            kingdomClan.Banner?.ChangePrimaryColor(kingdom.PrimaryBannerColor);
                            kingdomClan.Banner?.ChangeIconColors(kingdom.SecondaryBannerColor);
                        }
                    }
                    Log.write("Refreshing party icons");
                    foreach (var party in MobileParty.All)
                    {
                        if (party?.Party?.Owner?.Clan?.Kingdom == kingdom)
                        {
                            Log.write($"  Refresh {party.Name} icon");
                            party.Party.Visuals.SetMapIconAsDirty();
                        }
                    }
                    Log.write("Refreshing settlement icons(?)");
                    foreach (var settlement in kingdom.Settlements)
                    {
                        Log.write($"  Refresh {(settlement.IsTown ? "Town" : settlement.IsCastle ? "Castle" : settlement.IsVillage ? "Village" : "??")} {settlement} icon");
                        settlement.Party.Visuals.SetMapIconAsDirty();
                    }
                }
            }
            catch (Exception ex)
            {
                Log.write("Error applying clan colors");
                InformationManager.DisplayMessage(new InformationMessage("Error applying clan colors", new Color(1f, 0, 0)));
                Log.write(ex);
            }
        }
    }
}
