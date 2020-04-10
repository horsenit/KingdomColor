using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KingdomColor
{
    public class Settings
    {
        public bool OnlyPlayerRuledKingdoms { get; set; } = true;

        public static Settings Instance { get; private set; } = new Settings();

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
