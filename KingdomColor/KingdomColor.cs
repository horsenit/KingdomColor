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
using System.Globalization;

namespace KingdomColor
{
    public class KingdomColorModule : MBSubModuleBase
    {
        public static KingdomColorModule Instance;

        public void SetKingdomColors(Kingdom kingdom, uint primaryBannerColor, uint secondaryBannerColor, uint color, uint color2)
        {
            try
            {
                var playerClan = Clan.PlayerClan;
                var k = Traverse.Create(kingdom);
                k.Property<uint>("Color").Value = color;
                k.Property<uint>("Color2").Value = color2;
                k.Property<uint>("PrimaryBannerColor").Value = primaryBannerColor;
                k.Property<uint>("SecondaryBannerColor").Value = secondaryBannerColor;

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

        // HTML color #ffffff or banner color 123
        public static uint ParseUniformColor(string color)
        {
            const uint INVALID_COLOR = 0xdeadbeef;
            color = color.Trim();
            int bannerColor;
            if (int.TryParse(color, out bannerColor))
            {
                return BannerManager.GetColor(bannerColor);
            }
            if (color.Length < 7 || color[0] != '#') return INVALID_COLOR;
            try
            {
                byte r = byte.Parse(color.Substring(1, 2), NumberStyles.HexNumber);
                byte g = byte.Parse(color.Substring(3, 2), NumberStyles.HexNumber);
                byte b = byte.Parse(color.Substring(5, 2), NumberStyles.HexNumber);
                byte a = color.Length == 9 ? byte.Parse(color.Substring(7, 2), NumberStyles.HexNumber) : (byte)255;
                return ((uint)a << 24) | ((uint)r << 16) | ((uint)g << 8) | ((uint)b);
            }
            catch
            {
                return INVALID_COLOR;
            }
        }

        public static bool ShouldReplaceKingdomColor(Clan playerClan)
        {
            return playerClan != null && playerClan.Kingdom != null && (!Settings.Instance.OnlyPlayerRuledKingdoms || playerClan.Kingdom.RulingClan == playerClan);
        }

        public static (uint, uint, uint, uint) GetOverrideColors(Kingdom kingdom, uint primaryBannerColor, uint secondaryBannerColor, uint color, uint color2)
        {
            if (Settings.Instance.UseFactionColorOverrides)
            {
                var info = Settings.Instance.GetFactionColorOverride(kingdom);
                if (info.HasValue)
                    (primaryBannerColor, secondaryBannerColor) = info.Value;
            }
            if (Settings.Instance.UseUniformColorOverrides)
            {
                var info = Settings.Instance.GetUniformColorOverride(kingdom);
                if (info.HasValue)
                    (color, color2) = info.Value;
            }
            return (primaryBannerColor, secondaryBannerColor, color, color2);
        }

        public void SetClanKingdomColors(Clan playerClan, uint clanColor1, uint clanColor2)
        {
            if (ShouldReplaceKingdomColor(playerClan))
            {
                var kingdom = playerClan.Kingdom;
                var (primaryBannerColor, secondaryBannerColor, color, color2) =
                    GetOverrideColors(kingdom, clanColor1, clanColor2, clanColor1, clanColor2);
                SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color, color2);
            }
        }

        void ApplyOverrides()
        {
            if (Campaign.Current == null)
            {
                return;
            }

            if (Settings.Instance.UseFactionColorOverrides || Settings.Instance.UseUniformColorOverrides)
            {
                var kingdoms = Campaign.Current.Kingdoms;
                foreach (var kingdom in kingdoms)
                {
                    var (primaryBannerColor, secondaryBannerColor, color, color2) =
                        GetOverrideColors(kingdom, kingdom.PrimaryBannerColor, kingdom.SecondaryBannerColor, kingdom.Color, kingdom.Color2);
                    SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color, color2);
                }
            }
            // Now reapply player clan
            var playerClan = Clan.PlayerClan;
            if (playerClan != null)
            {
                SetClanKingdomColors(playerClan, playerClan.Color, playerClan.Color2);
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
                return "Usage: \"kingdomcolor.set_kingdom_color [KingdomId] [ColorId] [ColorId] [UniformColor] [UniformColor]\"\nUniformColors are optional and can be an HTML color ('#ffffff') or color id\nUse \"kingdomcolor.set_kingdom_color colors/kingdoms\" to list available colors or kingdoms";
            }
            var kingdom = MBObjectManager.Instance.GetObject<Kingdom>(args[0]);
            if (kingdom == null) return "Couldn't find kingdom.";
            int bannerColor;
            int bannerColor2;
            if (!int.TryParse(args[1], out bannerColor)) return "Invalid color1 specified";
            if (!int.TryParse(args[2], out bannerColor2)) return "Invalid color2 specified";
            uint primaryBannerColor = BannerManager.GetColor(bannerColor);
            uint secondaryBannerColor = BannerManager.GetColor(bannerColor2);
            uint? color1 = null;
            uint? color2 = null;
            if (args.Count >= 4)
            {
                color1 = ParseUniformColor(args[3]);
                color2 = ParseUniformColor(args[4]);
            }
            Instance.SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color1 ?? primaryBannerColor, color2 ?? secondaryBannerColor);
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
