using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace KingdomColor
{
    public class KingdomColorModule : MBSubModuleBase
    {
        public static KingdomColorModule Instance;
        
        protected override void OnSubModuleLoad()
        {
            base.OnSubModuleLoad();
            Instance = this;
            Settings.Load();
            // And make sure the file exists to allow editing
            Settings.Save();
            var harmony = new Harmony("KingdomColor patches");
            harmony.PatchAll();
        }

        protected override void OnSubModuleUnloaded()
        {
            Instance = null;
        }
    }
}
