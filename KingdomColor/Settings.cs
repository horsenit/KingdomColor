using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;

namespace KingdomColor
{
    public class FactionColor
    {
        public string Faction;
        public int PrimaryColor;
        public int SecondaryColor;

        public FactionColor() { }
        public FactionColor(string Faction, int PrimaryColor, int SecondaryColor)
        {
            this.Faction = Faction;
            this.PrimaryColor = PrimaryColor;
            this.SecondaryColor = SecondaryColor;
        }

        public static implicit operator FactionColor((string, int, int) value)
        {
            return new FactionColor(value.Item1, value.Item2, value.Item3);
        }
    }

    public class UniformColor
    {
        public string Faction;
        public string Color;
        public string Color2;

        public UniformColor() { }
        public UniformColor(string Faction, string Color, string Color2)
        {
            this.Faction = Faction;
            this.Color = Color;
            this.Color2 = Color2;
        }

        public static implicit operator UniformColor((string, string, string) value)
        {
            return new UniformColor(value.Item1, value.Item2, value.Item3);
        }
    }

    public class ClanBanner
    {
        public string Clan;
        public string BannerCode;
        public bool FollowKingdomColors = false;

        public ClanBanner() { }
        public ClanBanner(string Clan, string BannerCode)
        {
            this.Clan = Clan;
            this.BannerCode = BannerCode;
        }

        public static implicit operator ClanBanner((string, string) value)
        {
            return new ClanBanner(value.Item1, value.Item2);
        }
    }

    public class Settings
    {
        public bool OnlyPlayerRuledKingdoms { get; set; } = true;
        public bool PlayerClanBannerFollowsKingdom { get; set; } = true;
        public bool UseFactionColorOverrides { get; set; } = false;


        [XmlElement]
        public List<FactionColor> FactionColorOverride { get; set; } = new List<FactionColor>();

        public (uint, uint)? GetFactionColorOverride(Kingdom kingdom)
        {
            var info = FactionColorOverride.LastOrDefault(co => co.Faction == kingdom.StringId || co.Faction.ToLowerInvariant() == kingdom.Name.ToString().ToLowerInvariant());
            if (info == null) return null;
            return (BannerManager.GetColor(info.PrimaryColor), BannerManager.GetColor(info.SecondaryColor));
        }

        public bool UseUniformColorOverrides { get; set; } = false;

        [XmlElement]
        public List<UniformColor> UniformColorOverride { get; set; } = new List<UniformColor>();

        public (uint, uint)? GetUniformColorOverride(Kingdom kingdom)
        {
            var info = UniformColorOverride.LastOrDefault(co => co.Faction == kingdom.StringId || co.Faction.ToLowerInvariant() == kingdom.Name.ToString().ToLowerInvariant());
            if (info == null) return null;
            return (KingdomColorModule.ParseUniformColor(info.Color), KingdomColorModule.ParseUniformColor(info.Color2));
        }

        public bool UseClanBannerOverrides { get; set; } = false;

        [XmlElement]
        public List<ClanBanner> ClanBannerOverride { get; set; } = new List<ClanBanner>();

        public ClanBanner GetClanBannerOverride(Clan clan)
        {
            var info = ClanBannerOverride.LastOrDefault(co => co.Clan == clan.StringId || co.Clan.ToLowerInvariant() == clan.Name.ToString().ToLowerInvariant());
            if (info == null) return null;
            return info;
        }

        // parameterless constructor for XmlSerializer
        public Settings() { }
        // Workaround for XmlSerializer behaviour on default initialized Lists
        public Settings(bool populateDefaults)
        {
            if (populateDefaults)
            {
                FactionColorOverride = new List<FactionColor>() {
                    ("aserai", 0, 1),
                    ("battania", 2, 3),
                    ("empire", 4, 5),
                    ("empire_s", 6, 7),
                    ("empire_w", 8, 9),
                    ("khuzait", 10, 11),
                    ("sturgia", 12, 13),
                    ("vlandia", 14, 15)
                };
                UniformColorOverride = new List<UniformColor>()
                {
                    ("playerland_kingdom", "#ff0000", "#0000ff"),
                    ("aserai", "#965228", "#4F2212"),
                    ("battania", "#2D3F1D", "#BFCBB0"),
                    ("empire", "#4E3A55", "#DE9953"),
                    ("empire_s", "#9382D0", "#DE9953"),
                    ("empire_w", "#9E5072", "#DE9953"),
                    ("khuzait", "#58888B", "#CCBB89"),
                    ("sturgia", "#1C2A50", "#949CCC"),
                    ("vlandia", "#5C2017", "#ECBA44"),
                };
                ClanBannerOverride = new List<ClanBanner>()
                {
                    ("dey Meroc", "16.142.116.1536.1100.764.764.0.0.0.503.116.116.600.600.770.1229.1.1.0.503.116.116.600.600.764.364.1.1.0.510.142.116.1319.800.762.1209.1.0.270.108.142.116.1328.703.764.534.1.1.0.434.155.116.775.675.1204.724.1.1.-15.434.155.116.775.675.324.724.1.0.15.438.142.116.1420.940.764.1114.1.1.0.301.155.116.1000.737.710.969.1.0.155.301.155.116.1000.737.812.970.1.0.205.457.142.116.551.473.764.1027.1.0.180.301.155.116.1000.737.839.1054.1.0.-115.301.155.116.1000.737.690.1045.1.0.115.457.142.116.713.473.770.964.1.0.180.301.155.116.1000.737.588.1034.1.0.100.301.155.116.1000.737.926.1044.1.0.-100.301.142.116.1000.737.726.981.1.0.170.301.142.116.1000.737.794.979.1.0.190.304.142.116.1000.670.1064.1009.1.1.-70.304.142.116.1000.670.464.1009.1.0.70.445.155.116.800.800.760.1254.1.0.180.133.155.116.700.630.764.364.1.1.0.314.155.116.700.630.764.514.1.1.0.433.155.116.240.290.574.514.1.1.10.433.155.116.240.290.954.514.1.0.-6.503.142.116.800.800.764.764.1.1.0.503.116.116.720.720.764.764.1.1.0.503.116.155.400.400.764.764.1.1.0.503.142.155.370.370.764.764.1.1.0.423.155.116.694.628.764.775.1.1.0.445.155.116.95.80.765.464.1.0.0.523.155.116.60.65.764.504.0.0.0.401.155.116.425.425.764.764.1.1.15.401.155.116.325.325.764.759.1.1.0.401.155.116.230.230.764.764.1.1.15.401.155.116.150.150.764.764.1.1.0.401.155.116.115.115.764.764.1.1.15.401.155.116.75.75.764.764.1.1.0"),
                };
            }
        }

        public static Settings Instance { get; private set; } = new Settings(true);

        public static bool Load()
        {
            return Settings.Load(Settings.ConfigPath);
        }

        public static bool Load(string path)
        {
            try
            {
                if (File.Exists(Settings.ConfigPath))
                {
                    using (var writer = new StreamReader(new FileStream(path, FileMode.Open, FileAccess.Read)))
                    {
                        var serializer = new XmlSerializer(typeof(Settings));
                        Instance = (Settings)serializer.Deserialize(writer);
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                Log.write(ex);
            }
            return false;
        }

        static string ConfigPath
        {
            get
            {
                var dir = Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location));
                return Path.Combine(dir, "../..", "settings.xml");
            }
        }

        public static void Save()
        {
            Instance.Save(Settings.ConfigPath);
        }

        public void Save(string path)
        {
            using (var writer = new StreamWriter(new FileStream(path, FileMode.Create, FileAccess.Write)))
            {
                var serializer = new XmlSerializer(typeof(Settings));
                serializer.Serialize(writer, this);
            }
        }
    }
}
