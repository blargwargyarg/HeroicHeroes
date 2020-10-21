using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.MountAndBlade;
using HarmonyLib;

namespace HeroicHeroes
{
    class SubModule : MBSubModuleBase
    {
        protected override void OnSubModuleLoad()
        {
            try
            {
                base.OnSubModuleLoad();
                new Harmony("blargwarg.heroicheroes").PatchAll();
            }
            catch (Exception arg)
            {
                MessageBox.Show(string.Format("Failed to initialize Heroic heroes:\n\n{0}", arg));
            }
        }
    }
}
