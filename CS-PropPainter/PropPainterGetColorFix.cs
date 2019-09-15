using System;
using ColossalFramework.Math;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterGetColorFix
    {
        public Color GetColor(PropInfo prop, ushort propID, ref Randomizer r)
        {
            if (PropPainterManager.instance.map.ContainsKey(propID)) return PropPainterManager.instance.map[propID];
            else return prop.GetColor(ref r);
        }
    }
}
