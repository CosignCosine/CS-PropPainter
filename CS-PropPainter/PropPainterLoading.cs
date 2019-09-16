using System;
using ColossalFramework;
using ICities;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterLoading : ILoadingExtension
    {
        public static GameObject Container;

        public void OnCreated(ILoading loading)
        {
            if (PropPainterManager.instance == null)
            {
                Container = new GameObject("PropPainterManager");
                PropPainterManager.instance = Container.AddComponent<PropPainterManager>();
            }
        }

        public void OnLevelLoaded(LoadMode mode)
        {
            
        }

        public void OnLevelUnloading()
        {
            throw new NotImplementedException();
        }

        public void OnReleased()
        {
            throw new NotImplementedException();
        }
    }
}
