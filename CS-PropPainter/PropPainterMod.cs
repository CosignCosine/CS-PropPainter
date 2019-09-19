using ICities;
using System.Reflection;
using Harmony;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterMod : IUserMod
    {
        private readonly string harmonyId = "elektrix.proppainter";
        private HarmonyInstance harmony;

        public string Name => "Prop Painter";
        public string Description => "Painter, but for props.";

        public void OnEnabled()
        {
            //HarmonyInstance.DEBUG = false;
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnDisabled()
        {
            harmony.UnpatchAll(harmonyId);
            harmony = null;
        }

        public void OnSettingsUI(UIHelperBase helper){
            helper.AddCheckbox("Debug", false, (check) =>
            {
                Db.ON = check;
            });
        }
    }

    public static class Db {
        public static bool ON = false;

        public static void l(object m){
            if(ON) Debug.Log(m);
        }

        public static void w(object m)
        {
            if (ON) Debug.LogWarning(m);
        }

        public static void e(object m)
        {
            if (ON) Debug.LogError(m);
        }
    }
}
