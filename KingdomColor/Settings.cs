using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TaleWorlds.CampaignSystem;

namespace KingdomColor
{
    public struct FactionColor
    {
        public string Faction;
        public int PrimaryColor;
        public int SecondaryColor;

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

    public class Settings
    {
        public bool OnlyPlayerRuledKingdoms { get; set; } = true;
        public bool UseFactionColorOverrides { get; set; } = false;

        [XmlElement]
        public List<FactionColor> FactionColorOverride { get; set; } = new List<FactionColor>();

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
