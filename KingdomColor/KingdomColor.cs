using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using HarmonyLib;
using TaleWorlds.Core;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;

namespace KingdomColor
{
    public class KingdomColorModule : MBSubModuleBase
    {
        public static KingdomColorModule Instance;

        public void SetKingdomColors(Kingdom kingdom, uint color1, uint color2)
        {
            try
            {
                var playerClan = Clan.PlayerClan;
                var k = Traverse.Create(kingdom);
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
                // Does this do anything helpful? Harmful?
                foreach (var party in MobileParty.All)
                {
                    if (party.Party.Owner?.Clan?.Kingdom == kingdom)
                    {
                        party.Party.Visuals?.SetMapIconAsDirty();
                    }
                }
                foreach (var settlement in kingdom.Settlements)
                {
                    settlement.Party.Visuals?.SetMapIconAsDirty();
                }
            }
            catch (Exception ex)
            {
                Log.write("Error applying clan colors");
                InformationManager.DisplayMessage(new InformationMessage($"Error applying clan colors to {kingdom.Name}", new Color(1f, 0, 0)));
                Log.write(ex);
            }
        }

        public static bool ShouldReplaceKingdomColor(Clan playerClan)
        {
            return playerClan.Kingdom != null && (!Settings.Instance.OnlyPlayerRuledKingdoms || playerClan.Kingdom.RulingClan == playerClan);
        }

        void ApplyOverrides()
        {
            if (Settings.Instance.UseFactionColorOverrides)
            {
                foreach (var overrideInfo in Settings.Instance.FactionColorOverride)
                {
                    var faction = overrideInfo.Faction;
                    var color1 = BannerManager.GetColor(overrideInfo.PrimaryColor);
                    var color2 = BannerManager.GetColor(overrideInfo.SecondaryColor);
                    if (faction == null) continue;
                    var kingdoms = Campaign.Current.Kingdoms;
                    foreach (var kingdom in kingdoms)
                    {
                        if (kingdom.StringId == faction || kingdom.Name.ToString().ToLower() == faction.ToLower())
                        {
                            SetKingdomColors(kingdom, color1, color2);
                        }
                    }
                }
            }
            // Now reapply player clan
            var playerClan = Clan.PlayerClan;
            if (ShouldReplaceKingdomColor(playerClan))
            {
                SetKingdomColors(playerClan.Kingdom, playerClan.Color, playerClan.Color2);
            }
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_kingdom_color", "kingdomcolor")]
        public static string Console_SetKingdomColor(List<string> args)
        {
            if (args.Count == 1 && args[0] == "colors")
            {
                string output = $"\nAvailable colors\n==============================\n";
                foreach (var paletteEntry in BannerManager.ColorPalette)
                {
                    var c = Color.FromUint(paletteEntry.Value.Color);
                    output += $" Id: {paletteEntry.Key}, {c.ToString()}, rgba({(int)(c.Red * 255f)}, {(int)(c.Green * 255)}, {(int)(c.Blue * 255)}, {c.Alpha})\n";
                }
                return output;
            }
            else if (args.Count == 1 && args[0] == "kingdoms")
            {
                string output = "\nAvailable kingdoms\n==============================\n";
                foreach (var objectType in MBObjectManager.Instance.GetObjectTypeList<Kingdom>())
                    output = output + $" Id: {objectType.StringId} Name: {objectType.Name}\n";
                return output;
            }
            else if (args.Count < 3)
            {
                return "Usage: \"kingdomcolor.set_kingdom_color [KingdomId] [ColorId] [ColorId]\"\nUse \"kingdomcolor.set_kingdom_color colors/kingdoms\" to list available colors or kingdoms";
            }
            var kingdom = MBObjectManager.Instance.GetObject<Kingdom>(args[0]);
            if (kingdom == null) return "Couldn't find kingdom.";
            int color1;
            int color2;
            if (!int.TryParse(args[1], out color1)) return "Invalid color1 specified";
            if (!int.TryParse(args[2], out color2)) return "Invalid color2 specified";
            uint setColor1 = BannerManager.GetColor(color1);
            uint setColor2 = BannerManager.GetColor(color2);
            Instance.SetKingdomColors(kingdom, setColor1, setColor2);
            return $"Set {kingdom.Name} colors. Open and close the Clan page to take effect.";
        }
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;
            Settings.Load();
            // And make sure the file exists to allow editing
            Settings.Save();
            var harmony = new Harmony("KingdomColor patches 😎");
            harmony.PatchAll();
        }

        public override void OnGameInitializationFinished(Game game)
        {
            Settings.Load();
            ApplyOverrides();
        }

        protected override void OnSubModuleUnloaded()
        {
            Instance = null;
        }
    }
}
