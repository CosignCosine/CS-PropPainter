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

        // DEBUG ONLY
        private ushort a;
        private byte r;
        private byte g;
        private byte b;
        public void OnSettingsUI(UIHelperBase helper){
            helper.AddTextfield("ID", "0", (text) =>
            {
                a = ushort.Parse(text);
            });

            helper.AddTextfield("r", "0.0", (text) =>
            {
                r = byte.Parse(text);
            });

            helper.AddTextfield("g", "0.0", (text) =>
            {
                g = byte.Parse(text);
            });

            helper.AddTextfield("b", "0.0", (text) =>
            {
                b = byte.Parse(text);
            });

            helper.AddButton("color", () =>
            {
                if(PropPainterManager.instance == null){
                    Db.l("Prop Painter instance was null.");
                    return;
                }
                PropPainterManager.instance.SetColor(a, new Color32(r, g, b, 255));
            });
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
