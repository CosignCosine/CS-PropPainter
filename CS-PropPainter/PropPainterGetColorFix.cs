using System;
using ColossalFramework.Math;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterGetColorFix
    {
        public Color GetColor(PropInfo prop, ushort propID, ref Randomizer r)
        {
            // Quick fix: if the color is mandated by the mod then it is parsed, else the vanilla method is used.
            if (PropPainterManager.instance.map.ContainsKey(propID)) return PropPainterManager.instance.map[propID];
            else return prop.GetColor(ref r);
        }
    }
}
