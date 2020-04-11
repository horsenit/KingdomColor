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

    public class Settings
    {
        public bool OnlyPlayerRuledKingdoms { get; set; } = true;
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
