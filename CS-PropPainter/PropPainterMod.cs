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
}
