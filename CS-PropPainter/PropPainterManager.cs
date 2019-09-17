using System;
using ColossalFramework;
using ColossalFramework.UI;
using System.Collections.Generic;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterManager : MonoBehaviour
    {
        public static PropPainterManager instance;

        public Dictionary<ushort, SerializableColor> map = new Dictionary<ushort, SerializableColor>();

        public void SetColor(ushort prop, Color color){
            map.Add(prop, color);

            //(GameObject.Find("PropPainterManager").GetComponentInChildren(Type.GetType("PropPainter.PropPainterManager, CS-PropPainter")) as PropPainterManager).SetColor(14289, new Color32(124, 165, 255, 255));

            /* tests
            Type PPM = Type.GetType("PropPainter.PropPainterManager, CS-PropPainter");
            object PropPainterManager = Convert.ChangeType(GameObject.Find("PropPainterManager").GetComponentInChildren(PPM), PPM);
            System.Reflection.MethodInfo method = PPM.GetMethod("SetColor");
            object instance = PPM.GetField("instance").GetValue(PPM);
            method.Invoke(instance, new object[] { 14289, (Color) new Color32(124, 165, 255, 255) });
            */
            
        }

        public Color? GetColor(ushort prop){
            if (map.ContainsKey(prop)) return map[prop];
            else return null;
        }

        // Convert.ChangeType(GameObject.Find("PropPainterManager").GetComponentInChildren(Type.GetType("PropPainter.PropPainterManager, CS-PropPainter")), Type.GetType("PropPainter.PropPainterManager, CS-PropPainter"))

        public UIColorField colorField;
    }
}
