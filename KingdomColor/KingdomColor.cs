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
using TaleWorlds.MountAndBlade.View;
using TaleWorlds.InputSystem;

namespace KingdomColor
{
    public class KingdomColorModule : MBSubModuleBase
    {
        public static KingdomColorModule Instance;

        public void SetKingdomColors(Kingdom kingdom, uint primaryBannerColor, uint secondaryBannerColor, uint color, uint color2, Clan skipClan = null)
        {
            try
            {
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
                    if (kingdomClan == skipClan)
                    {
                        Log.write($"!!Skipping clan {skipClan}");
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

        public static (uint, uint, uint, uint) GetOverrideColors(IFaction kingdom, uint primaryBannerColor, uint secondaryBannerColor, uint color, uint color2)
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
                SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color, color2, playerClan);
            }
        }

        public bool SetClanBanner(Clan clan, string bannerCode)
        {
            try
            {
                if (bannerCode == null) return false;
                if (clan == null) return false;
                Log.write($"Trying to set {clan.Name}'s banner to {bannerCode}");
                // Try to fish out errors by forcing the deserialization and a render attempt before setting the clan's banner
                var bd = new Banner(bannerCode);
                var m = bd.ConvertToMultiMesh();
                clan.Banner.Deserialize(bannerCode);
                return true;
            }
            catch (Exception ex)
            {
                Log.write(ex);
                return false;
            }
        }

        public bool SetClanBanner(Clan clan, ClanBanner info)
        {
            if (info == null) return false;
            if (clan == null) return false;
            var bannerCode = info.BannerCode;
            if (SetClanBanner(clan, bannerCode))
            {
                if (info.FollowKingdomColors)
                {
                    Traverse.Create(clan).Method("UpdateBannerColorsAccordingToKingdom").GetValue();
                }
            }
            return true;
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
                    SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color, color2, Settings.Instance.PlayerClanBannerFollowsKingdom ? null : Clan.PlayerClan);
                }
                var clans = Campaign.Current.Clans;
                foreach (var clan in clans)
                {
                    var (primaryBannerColor, secondaryBannerColor, color, color2) =
                        GetOverrideColors(clan, clan.Color, clan.Color2, clan.Color, clan.Color2);
                    clan.Color = color;
                    clan.Color2 = color2;
                }
            }
            if (Settings.Instance.UseClanBannerOverrides)
            {
                var clans = Campaign.Current.Clans;
                foreach (var clan in clans)
                {
                    var clanBanner = Settings.Instance.GetClanBannerOverride(clan);
                    SetClanBanner(clan, clanBanner);
                }
            }
            // Now reapply player clan
            var playerClan = Clan.PlayerClan;
            if (playerClan != null)
            {
                SetClanKingdomColors(playerClan, playerClan.Color, playerClan.Color2);
            }
        }

        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            try
            {
                Instance = this;
                var harmony = new Harmony("KingdomColor patches 😎");
                harmony.PatchAll();
                Settings.Load();
                // And make sure the file exists to allow editing, including new defaults
                Settings.Save();
            }
            catch (Exception ex)
            {
                DelayMessage("KingdomColor encountered an error while initializing, details copied to clipboard.", Color.FromUint(0xffff0000));
                Input.SetClipboardText(FormatException(ex));
            }
        }

        static string FormatException(Exception ex)
        {
            return $"{ex.GetType().Name}: {ex.Message}\r\n{ex.StackTrace}" + (ex.InnerException != null ? "\r\n" + FormatException(ex.InnerException) : "");
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

        static List<(string, Color)> messages = new List<(string, Color)>();
        public static void DelayMessage(string message, Color? color = null)
        {
            messages.Add((message, color ?? Color.White));
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            foreach (var (message, color) in messages)
            {
                InformationManager.DisplayMessage(new InformationMessage(message, color));
            }
            messages.Clear();
        }
    }
}
