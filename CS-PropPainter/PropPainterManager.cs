using System;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.IO;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace PropPainter
{
    public class PropPainterManager : MonoBehaviour
    {
        public static PropPainterManager instance;

        public Dictionary<ushort, Color> map = new Dictionary<ushort, Color>();

        public void SetColor(ushort prop, Color color){
            if (map.ContainsKey(prop)) map[prop] = color;
            else map.Add(prop, color);
        }

        public Color? GetColor(ushort prop){
            if (map.ContainsKey(prop)) return map[prop];
            else return null;
        }

        public List<ushort> ExtractPropsFromMoveItSelection(){
            var Action = Type.GetType("MoveIt.Action, MoveIt");
            var xs = Action.GetField("selection", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static).GetValue(null);
            IEnumerable MoveItSelection = (IEnumerable) xs;

            List<ushort> formattedProps = new List<ushort>();

            foreach(object objectInstance in MoveItSelection){
                PropertyInfo id = objectInstance.GetType().GetProperty("id");
                InstanceID unreflected = (InstanceID)(id.GetValue(objectInstance, null));
                if (unreflected.Type == InstanceType.Prop)
                {
                    formattedProps.Add(unreflected.Prop);
                }
            }

            return formattedProps;
        }

        public Color? ParseAggregateColor(List<ushort> t){
            Color? theColor = null;
            for (int i = 0; i < t.Count; i++){
                Color? x = GetColor(t[i]);
                if(x != null){
                    if(theColor == null){
                        theColor = x;
                        continue;
                    }else if(theColor != x){
                        return null;
                    }
                }
            }
            return theColor;
        }

        public UIColorField colorField;

        public Vector3 colorFieldPosition = new Vector3(1.02f, -0.33f, 0f);
    }

    public class PropPainterDataContainer : IDataContainer {
        public static string DataId = "PropPainter";
        public static uint DataVersion = 1;
        public void Serialize(DataSerializer s)
        {
            // convert ushort list to int array
            int[] ids = new int[PropManager.MAX_PROP_COUNT];
            foreach (KeyValuePair<ushort, Color> item in PropPainterManager.instance.map)
            {
                Color c = item.Value;
                int hex = ((byte)c.r) << 16 | ((byte)c.b) << 8 | ((byte)c.g);
                Debug.Log(hex);
                ids[item.Key] = hex;
            }

            s.WriteInt32Array(ids);
        }

        // This reads the object (from bytes)
        public void Deserialize(DataSerializer s)
        {
            int[] ids = s.ReadInt32Array();

            for (ushort i = 0; i < ids.Length; i++){
                int h = ids[i];
                byte r = (byte)((h >> 16) & 0xFF);
                byte g = (byte)((h >> 8) & 0xFF);
                byte b = (byte)((h) & 0xFF);
                PropPainterManager.instance.SetColor(i, new Color32(r, g, b, 255));
            }
        }


        public void AfterDeserialize(DataSerializer s)
        {
            /*
            if (!PropManager.exists) return;

            List<ushort> invalidPropIDs = new List<ushort>();

            PropInstance[] propInstances = PropManager.instance.m_props.m_buffer;

            // itertate through all building ids, filter active ids
            foreach (KeyValuePair<ushort, Color> item in PropPainterManager.instance.map)
            {
                if ((PropInstance.Flags) propInstances[item.Key].m_flags == PropInstance.Flags.None)
                {
                    invalidPropIDs.Add(item.Key);
                }
            }

            PropPainterManager.instance.map
            */
        }
    }

    public class PropPainterSerializer : SerializableDataExtensionBase {
        private PropPainterDataContainer _data;

        public override void OnLoadData()
        {
            // Get bytes from savegame
            byte[] bytes = serializableDataManager.LoadData(PropPainterDataContainer.DataId);
            if (bytes != null)
            {
                // Convert the bytes to MakeHistoricalData object
                using (var stream = new MemoryStream(bytes))
                {
                    _data = DataSerializer.Deserialize<PropPainterDataContainer>(stream, DataSerializer.Mode.Memory);
                }

                Debug.LogFormat("Data loaded (Size in bytes: {0})", bytes.Length);
            }
            else
            {
                _data = new PropPainterDataContainer();

                Debug.Log("Data created");
            }
        }

        public override void OnSaveData()
        {
            byte[] bytes;

            // Convert the MakeHistoricalData object to bytes
            using (var stream = new MemoryStream())
            {
                DataSerializer.Serialize(stream, DataSerializer.Mode.Memory, PropPainterDataContainer.DataVersion, _data);
                bytes = stream.ToArray();
            }

            // Save bytes in savegame
            serializableDataManager.SaveData(PropPainterDataContainer.DataId, bytes);

            Debug.LogFormat("Data saved (Size in bytes: {0})", bytes.Length);
        }
    }
}
