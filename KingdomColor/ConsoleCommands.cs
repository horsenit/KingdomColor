﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;

namespace KingdomColor
{
    class ConsoleCommands
    {
        [CommandLineFunctionality.CommandLineArgumentFunction("set_kingdom_color", "kingdomcolor")]
        public static string SetKingdomColor(List<string> strings)
        {
            var args = ConsoleUtilities.Resplit(strings);
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
                return ConsoleUtilities.GetObjectList<Kingdom>();
            }
            else if (args.Count < 3)
            {
                return $@"Usage: ""kingdomcolor.set_kingdom_color [KingdomId/KingdomName] [ColorId] [ColorId] [UniformColor] [UniformColor]""
UniformColors are optional and can be an HTML color ('#ffffff') or color id
Use ""kingdomcolor.set_kingdom_color colors/kingdoms"" to list available colors or kingdoms";
            }
            var kingdom = ConsoleUtilities.FindObjectByIdName<Kingdom>(args[0]);
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
                color1 = KingdomColorModule.ParseUniformColor(args[3]);
                color2 = KingdomColorModule.ParseUniformColor(args[4]);
            }
            KingdomColorModule.Instance.SetKingdomColors(kingdom, primaryBannerColor, secondaryBannerColor, color1 ?? primaryBannerColor, color2 ?? secondaryBannerColor);
            return $"Set {kingdom.Name} colors. Open and close the Clan page to take effect.";
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("list_clans", "kingdomcolor")]
        public static string ListClans(List<string> args)
        {
            return ConsoleUtilities.GetObjectList<Clan>();
        }

        [CommandLineFunctionality.CommandLineArgumentFunction("set_clan_banner", "kingdomcolor")]
        public static string SetClanBanner(List<string> strings)
        {
            var args = ConsoleUtilities.Resplit(strings);
            if (args.Count == 1 && args[0] == "clans")
            {
                return ConsoleUtilities.GetObjectList<Clan>();
            }
            else if (args.Count < 2)
            {
                return $@"Usage: ""kingdomcolor.set_clan_banner [ClanName/ClanId] [BannerCode]""
You must quote clan names with spaces using single quotes: 'dey Meroc' for example
Use ""kingdomcolor.list_clans"" to see a list of clans";
            }

            var clan = ConsoleUtilities.FindObjectByIdName<Clan>(args[0]);
            if (clan == null) return $"Couldn't find clan {args[0]}.";
            var bannerCode = string.Join(" ", args.Skip(1));
            if (!KingdomColorModule.Instance.SetClanBanner(clan, bannerCode))
            {
                return "Invalid banner code.";
            }
            return $"Changed {clan.Name}'s banner. Open and close the Clan page to take effect.";
        }
    }
}