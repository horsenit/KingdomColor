using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using TaleWorlds.Library;
using TaleWorlds.ObjectSystem;

namespace KingdomColor
{
    class DefaultLoader
    {
        XmlDocument LoadId(string id)
        {
            var xmlFiles = XmlResource.XmlInformationList.Where(x => x.Id == id);
            return LoadXmls(xmlFiles);
        }

        // reimplementation of MBObjectManager.LoadXML(string id, Type typeOfGameMenusCallbacks = null, bool skipXmlFilterForEditor = false)
        // reimplemented to avoid unwanted callbacks and constructors,
        // just want a small slice of info, not for the game to reload the information.
        XmlDocument LoadXmls(IEnumerable<MbObjectXmlInformation> xmlFiles)
        {
            var xmlDocuments = new List<(string xmlPath, string xsdPath, string xsltPath)>();
            foreach (var xmlFile in xmlFiles)
            {
                var xmlPath = ModuleInfo.GetXmlPath(xmlFile.ModuleName, xmlFile.Name);
                if (File.Exists(xmlPath))
                {
                    var xsdPath = ModuleInfo.GetXsdPath(xmlFile.ModuleName, xmlFile.Id);
                    var xsltPath = ModuleInfo.GetXsltPath(xmlFile.ModuleName, xmlFile.Id);
                    xmlDocuments.Add((xmlPath, xsdPath, xsltPath));
                }
            }

            if (xmlDocuments.Count == 0)
                return null;

            var id = xmlFiles.First().Id;

            XmlDocument loadDocument(string xmlPath, string xsdPath) =>
                Traverse.Create(MBObjectManager.Instance).Method("CreateDocumentFromXmlFile", xmlPath, xsdPath, false).GetValue() as XmlDocument;

            var document = loadDocument(xmlDocuments[0].xmlPath, xmlDocuments[0].xsdPath);
            foreach (var (xmlPath, xsdPath, xsltPath) in xmlDocuments.Skip(1))
            {
                if (File.Exists(xsltPath))
                    document = Traverse.Create(MBObjectManager.Instance).Method("ApplyXslt", xsltPath, document).GetValue() as XmlDocument;
                var newDocument = loadDocument(xmlPath, xsdPath);
                document = Traverse.Create(MBObjectManager.Instance).Method("MergeTwoXmls", document, newDocument, id).GetValue() as XmlDocument;
            }
            return document;
        }

        public Settings Settings { get; protected set; }

        void processClans(XmlDocument document)
        {
            var nodes = document.SelectNodes("/Factions/Faction");
            foreach (XmlNode node in nodes)
            {
                string attr(string attrName) => node.Attributes[attrName]?.Value;
                var id = attr("id");
                if (id == "player_faction")
                    continue;

                var banner = attr("banner_key") ?? Banner.CreateRandomClanBanner(id.GetDeterministicHashCode()).Serialize();
                var color = attr("color");
                if (color?.Trim()?.Length > 0) color = Color.FromUint(Convert.ToUInt32(color, 16)).ToString();
                var color2 = attr("color2");
                if (color2?.Trim()?.Length > 0) color2 = Color.FromUint(Convert.ToUInt32(color2, 16)).ToString();

                if (color != null || color2 != null)
                    Settings.UniformColorOverride.Add(new UniformColor(id, color, color2));
                Settings.ClanBannerOverride.Add(new ClanBanner(id, banner, true));
            }
        }

        void processKingdoms(XmlDocument document)
        {
            var nodes = document.SelectNodes("/Kingdoms/Kingdom");
            foreach (XmlNode node in nodes)
            {
                string attr(string attrName) => node.Attributes[attrName]?.Value;
                var id = attr("id");
                var banner = attr("banner_key") ?? Banner.CreateRandomClanBanner(id.GetDeterministicHashCode()).Serialize();
                var color = attr("color");
                if (color?.Trim()?.Length > 0) color = Color.FromUint(Convert.ToUInt32(color, 16)).ToString();
                var color2 = attr("color2");
                if (color2?.Trim()?.Length > 0) color2 = Color.FromUint(Convert.ToUInt32(color2, 16)).ToString();
                var primaryBannerColor = BannerManager.GetColorId(Convert.ToUInt32(attr("primary_banner_color") ?? "-1", 16));
                var secondaryBannerColor = BannerManager.GetColorId(Convert.ToUInt32(attr("secondary_banner_color") ?? "-1", 16));

                Settings.UniformColorOverride.Add(new UniformColor(id, color, color2));
                Settings.FactionColorOverride.Add(new FactionColor(id, primaryBannerColor, secondaryBannerColor));

                // get ruling clan banner
                var rulingClan = MBObjectManager.Instance.ReadObjectReferenceFromXml<Hero>("owner", node)?.Clan;
                if (rulingClan != null)
                {
                    Settings.ClanBannerOverride.Add(new ClanBanner(rulingClan.StringId, banner, true));
                }
            }
        }

        protected DefaultLoader()
        {
            Settings = new Settings(false);
            processClans(LoadId("Factions"));
            processKingdoms(LoadId("Kingdoms"));
            Settings.UseClanBannerOverrides = true;
            Settings.UseFactionColorOverrides = true;
            Settings.UseUniformColorOverrides = true;
            Settings.OnlyPlayerRuledKingdoms = Settings.Instance.OnlyPlayerRuledKingdoms;
            Settings.PlayerClanBannerFollowsKingdom = Settings.Instance.PlayerClanBannerFollowsKingdom;
        }

        public static Settings Load()
        {
            var loader = new DefaultLoader();
            return loader.Settings;
        }
    }
}
