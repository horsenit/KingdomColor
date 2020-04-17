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
using TaleWorlds.Library;

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
        public ClanBanner(string Clan, string BannerCode, bool FollowKingdomColors)
        {
            this.Clan = Clan;
            this.BannerCode = BannerCode;
            this.FollowKingdomColors = FollowKingdomColors;
        }

        public static implicit operator ClanBanner((string, string, bool) value)
        {
            return new ClanBanner(value.Item1, value.Item2, value.Item3);
        }
    }

    public class Settings
    {
        public bool OnlyPlayerRuledKingdoms { get; set; } = true;
        public bool PlayerClanBannerFollowsKingdom { get; set; } = true;
        public bool UseFactionColorOverrides { get; set; } = false;


        [XmlElement]
        public List<FactionColor> FactionColorOverride { get; set; } = new List<FactionColor>();

        public (uint, uint)? GetFactionColorOverride(IFaction kingdom)
        {
            var info = FactionColorOverride.LastOrDefault(co => co.Faction == kingdom.StringId || co.Faction.ToLowerInvariant() == kingdom.Name.ToString().ToLowerInvariant());
            if (info == null) return null;
            return (BannerManager.GetColor(info.PrimaryColor), BannerManager.GetColor(info.SecondaryColor));
        }

        public bool UseUniformColorOverrides { get; set; } = false;

        [XmlElement]
        public List<UniformColor> UniformColorOverride { get; set; } = new List<UniformColor>();

        public (uint, uint)? GetUniformColorOverride(IFaction kingdom)
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
            var info = ClanBannerOverride.LastOrDefault(co => co.Clan == clan.StringId || co.Clan.ToLowerInvariant() == clan.Name?.ToString().ToLowerInvariant());
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
                    ("dey Meroc", "16.142.116.1536.1100.764.764.0.0.0.503.116.116.600.600.770.1229.1.1.0.503.116.116.600.600.764.364.1.1.0.510.142.116.1319.800.762.1209.1.0.270.108.142.116.1328.703.764.534.1.1.0.434.155.116.775.675.1204.724.1.1.-15.434.155.116.775.675.324.724.1.0.15.438.142.116.1420.940.764.1114.1.1.0.301.155.116.1000.737.710.969.1.0.155.301.155.116.1000.737.812.970.1.0.205.457.142.116.551.473.764.1027.1.0.180.301.155.116.1000.737.839.1054.1.0.-115.301.155.116.1000.737.690.1045.1.0.115.457.142.116.713.473.770.964.1.0.180.301.155.116.1000.737.588.1034.1.0.100.301.155.116.1000.737.926.1044.1.0.-100.301.142.116.1000.737.726.981.1.0.170.301.142.116.1000.737.794.979.1.0.190.304.142.116.1000.670.1064.1009.1.1.-70.304.142.116.1000.670.464.1009.1.0.70.445.155.116.800.800.760.1254.1.0.180.133.155.116.700.630.764.364.1.1.0.314.155.116.700.630.764.514.1.1.0.433.155.116.240.290.574.514.1.1.10.433.155.116.240.290.954.514.1.0.-6.503.142.116.800.800.764.764.1.1.0.503.116.116.720.720.764.764.1.1.0.503.116.155.400.400.764.764.1.1.0.503.142.155.370.370.764.764.1.1.0.423.155.116.694.628.764.775.1.1.0.445.155.116.95.80.765.464.1.0.0.523.155.116.60.65.764.504.0.0.0.401.155.116.425.425.764.764.1.1.15.401.155.116.325.325.764.759.1.1.0.401.155.116.230.230.764.764.1.1.15.401.155.116.150.150.764.764.1.1.0.401.155.116.115.115.764.764.1.1.15.401.155.116.75.75.764.764.1.1.0", true),
                    ("osticos", "11.35.40.3000.3000.764.764.1.0.0.427.35.35.39.16.714.712.0.1.184.504.95.95.42.230.594.764.0.0.90.504.143.95.38.226.612.764.0.0.90.504.95.95.42.230.934.764.0.0.270.504.143.95.38.226.916.764.0.0.270.504.95.95.42.230.848.906.0.1.210.504.143.95.38.226.839.890.0.1.210.504.95.95.42.230.680.906.0.0.150.504.143.95.38.226.689.890.0.0.150.504.95.95.42.230.680.609.0.0.30.504.143.95.38.226.689.624.0.0.30.504.95.95.42.230.848.609.0.0.330.504.143.95.38.226.839.625.0.0.330.505.95.35.62.101.639.807.0.0.301.503.95.35.71.91.661.830.0.0.119.503.95.35.72.88.593.844.0.0.155.505.143.35.56.95.639.807.0.0.301.503.143.35.65.84.661.830.0.0.119.503.143.35.62.84.593.844.0.0.155.216.95.35.237.64.683.828.0.0.12.216.95.35.242.64.656.791.0.0.13.216.95.35.256.119.683.804.0.1.236.503.95.35.91.81.611.889.0.0.299.503.40.35.94.85.613.896.0.0.299.505.95.35.62.101.651.682.0.0.242.503.95.35.71.91.642.712.0.0.60.503.95.35.72.88.596.661.0.0.96.505.143.35.56.95.651.682.0.0.242.503.143.35.65.84.642.712.0.0.60.503.143.35.62.84.596.661.0.0.96.216.95.35.237.64.656.730.0.0.313.216.95.35.242.64.673.688.0.0.313.216.95.35.256.119.676.718.0.1.177.503.95.35.91.81.566.699.0.0.240.503.40.35.94.85.561.704.0.0.240.505.95.35.62.101.745.619.0.1.178.503.95.35.71.91.776.626.0.1.0.503.95.35.72.88.754.561.0.1.324.505.143.35.56.95.745.619.0.1.178.503.143.35.65.84.776.626.0.1.0.503.143.35.62.84.754.561.0.1.324.216.95.35.237.64.790.677.0.1.107.216.95.35.242.64.745.672.0.1.106.216.95.35.256.119.769.689.0.0.243.503.95.35.91.81.802.554.0.1.180.503.40.35.94.85.809.552.0.1.180.505.95.35.62.101.891.709.0.0.121.503.95.35.71.91.869.685.0.0.-61.503.95.35.72.88.937.672.0.0.335.505.143.35.56.95.891.709.0.0.121.503.143.35.65.84.869.685.0.0.-61.503.143.35.62.84.937.672.0.0.335.216.95.35.237.64.825.698.0.0.192.216.95.35.242.64.851.734.0.0.193.216.95.35.256.119.825.722.0.1.56.503.95.35.91.81.919.627.0.0.119.503.40.35.94.85.917.620.0.0.119.505.95.35.62.101.863.840.0.0.62.503.95.35.71.91.872.810.0.0.-120.503.95.35.72.88.918.861.0.0.276.505.143.35.56.95.863.840.0.0.62.503.143.35.65.84.872.810.0.0.-120.503.143.35.62.84.918.861.0.0.276.216.95.35.237.64.838.782.0.0.133.216.95.35.242.64.840.833.0.0.133.216.95.35.256.73.812.814.0.1.135.503.95.35.91.81.947.823.0.0.60.503.40.35.94.85.953.818.0.0.60.505.95.35.62.101.747.888.0.0.2.503.95.35.71.91.778.882.0.0.-180.503.95.35.72.88.756.947.0.0.216.505.143.35.56.95.747.888.0.0.2.503.143.35.65.84.778.882.0.0.-180.503.143.35.62.84.756.947.0.0.216.216.95.35.237.64.787.861.0.0.73.216.95.35.242.64.741.866.0.0.73.216.95.35.256.119.766.849.0.1.297.503.95.35.91.81.804.954.0.0.0.503.40.35.94.85.811.956.0.0.0.216.95.35.256.51.806.684.0.1.233.216.95.35.256.-57.730.694.0.1.306.216.95.35.132.56.652.782.0.1.286.216.95.35.230.41.708.848.0.1.252.216.95.35.230.-41.708.848.0.1.235.216.95.35.230.41.812.850.0.1.312.216.95.35.230.-41.812.850.0.1.295.216.95.35.132.56.868.780.0.0.73.503.95.35.245.245.764.756.0.0.-180.503.143.35.240.240.764.756.0.0.-180.503.95.35.82.85.811.723.0.0.0.503.143.35.90.90.813.731.0.0.0.503.5.35.21.20.811.725.0.0.0.503.95.35.82.85.717.723.0.0.0.503.143.35.90.90.715.731.0.0.0.503.5.35.20.71.764.749.0.0.0.505.95.35.4.53.764.748.0.0.0.503.95.35.16.16.750.778.0.0.0.503.143.35.18.16.753.781.0.0.0.503.5.35.7.7.749.776.0.0.0.503.95.35.10.10.753.780.0.0.0.503.95.35.16.16.778.778.0.0.0.503.143.35.18.16.775.781.0.0.0.503.5.35.10.10.779.776.0.0.0.503.95.35.10.10.775.780.0.0.0.503.95.35.22.20.719.725.0.0.0.505.95.35.40.12.736.796.0.0.6.503.95.35.32.51.808.818.0.0.0.503.143.35.44.44.801.829.0.0.0.505.95.35.40.12.792.802.0.0.-353.505.95.35.40.12.792.796.0.0.-6.503.5.35.36.7.826.717.0.0.0.503.5.35.36.7.730.716.0.0.0.503.5.35.14.14.702.726.0.0.0.427.95.35.50.16.715.731.0.0.0.427.95.35.50.16.715.721.0.1.180.427.95.35.40.12.728.714.0.1.173.427.95.35.40.12.826.714.0.0.173.427.95.35.50.16.813.721.0.1.180.427.95.35.50.16.813.731.0.0.0.503.95.35.22.20.817.725.0.0.0.503.95.35.15.25.771.800.0.0.27.503.95.35.15.25.757.800.0.0.-27.503.95.35.32.51.720.818.0.0.0.503.143.35.44.44.727.829.0.0.0.505.95.35.40.12.736.802.0.0.-7.503.5.35.35.16.763.823.0.0.0.427.95.35.56.18.764.820.0.0.-180.427.95.35.21.15.795.822.0.1.0.427.95.35.21.15.733.822.0.0.0.427.95.35.19.12.766.833.0.0.158.503.5.35.6.8.718.724.0.0.0.503.5.35.4.4.821.722.0.0.0.427.95.35.11.8.764.783.0.0.0.504.143.35.35.76.764.695.0.0.0", false),
                };
            }
        }

        public static Settings Instance { get; set; } = new Settings(true);

        public static bool Load()
        {
            if (File.Exists(Settings.OldConfigPath))
            {
                KingdomColorModule.DelayMessage("KingdomColor: Loading settings from Modules\\KingdomColor\\settings.xml instead of Documents\\Mount and Blade II Bannerlord\\Configs\\KingdomColor.xml", Color.FromUint(0xffff8000));
                return Settings.Load(Settings.OldConfigPath);
            }
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

        static string ConfigPath => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Mount and Blade II Bannerlord", "Configs", "KingdomColor.xml");
        static string OldConfigPath => Path.Combine(Path.GetDirectoryName(Path.GetFullPath(Assembly.GetExecutingAssembly().Location)), "..", "..", "settings.xml");

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
