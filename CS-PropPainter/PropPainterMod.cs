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
            HarmonyInstance.DEBUG = true;
            harmony = HarmonyInstance.Create(harmonyId);
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        public void OnDisabled()
        {
            harmony.UnpatchAll(harmonyId);
            harmony = null;
        }
    }

    public static class Db {
        public static bool ON = true;

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
