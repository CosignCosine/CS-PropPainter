using System;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterLoading : LoadingExtensionBase
    {
        public static GameObject Container;

        public override void OnCreated(ILoading loading)
        {
            if (PropPainterManager.instance == null)
            {
                Container = new GameObject("PropPainterManager");
                PropPainterManager.instance = Container.AddComponent<PropPainterManager>();
            }
        }
    }
}
