using System;
using System.Linq;
using ICities;
using ColossalFramework.UI;
using ColossalFramework.IO;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System.Reflection;
using UnityEngine;
using MoveIt;

namespace PropPainter
{
    public class PropPainterManager : MonoBehaviour
    {
        // Static instance initialized at runtime
        public static PropPainterManager instance;

        // Public map because that works too
        public Dictionary<ushort, Color> map = new Dictionary<ushort, Color>();

        // Getting and setting colors
        public void SetColor(ushort prop, Color color){
            if (map.ContainsKey(prop)) map[prop] = color;
            else map.Add(prop, color);
        }

        public Color? GetColor(ushort prop){
            if (map.ContainsKey(prop)) return map[prop];
            else return null;
        }

        // Acquiring and parsing move it selections
        public List<ushort> ExtractPropsFromMoveItSelection(){
            /*
            var Action = Type.GetType("MoveIt.Action, MoveIt");
            var xs = Action.GetField("selection", BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static).GetValue(null);
            IEnumerable MoveItSelection = (IEnumerable) xs;*/
            HashSet<Instance> MoveItSelection = MoveIt.Action.selection;

            List<ushort> formattedProps = new List<ushort>();

            //foreach(object objectInstance in MoveItSelection){
            foreach(Instance objectInstance in MoveItSelection){
                /*
                PropertyInfo id = objectInstance.GetType().GetProperty("id");
                InstanceID unreflected = (InstanceID)(id.GetValue(objectInstance, null));
                */
                InstanceID unreflected = objectInstance.id;
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

        // UI
        public UIColorField colorField;
        public UIColorPicker colorPicker;
        public UIMultiStateButton propPainterButton;
        public UIPanel colorPanel;
    }

    public class PropPainterDataContainer : IDataContainer {
        public static string DataId = "PropPainter";
        public static uint DataVersion = 1;

        public void Serialize(DataSerializer s)
        {
            // convert ushort list to int array
            int[] ids = new int[PropManager.MAX_PROP_COUNT];
            for (int i = 0; i < ids.Length; i++) {
                ids[i] = 16777216;
            }
            foreach (KeyValuePair<ushort, Color> item in PropPainterManager.instance.map)
            {
                Color32 c = item.Value;
                int hex = ((byte)c.r) << 16 | ((byte)c.g) << 8 | ((byte)c.b);
                Debug.Log(hex);
                ids[item.Key] = hex;
            }

            s.WriteInt32Array(ids);
        }

        // This reads the object (from bytes)
        public void Deserialize(DataSerializer s)
        {
            int[] ids = s.ReadInt32Array();

            PropPainterManager.instance.map = new Dictionary<ushort, Color>();

            // Temporary hack. I need to figure out why this for loop repeats forever, I cannot figure out why.
            List<int> repeatedIDs = new List<int>();

            for (ushort i = 0; i < ids.Length; i++){
                Debug.Log(i);

                int h = ids[i];
                if (repeatedIDs.Contains(i))
                {
                    break;
                }

                if (h != 16777216 && !repeatedIDs.Contains(i)){

                    repeatedIDs.Add(i);

                    byte r = (byte)((h >> 16) & 0xFF);
                    byte g = (byte)((h >> 8) & 0xFF);
                    byte b = (byte)((h) & 0xFF);
                    PropPainterManager.instance.SetColor(i, new Color32(r, g, b, 255));
                }
            }
        }


        public void AfterDeserialize(DataSerializer s)
        {
            // @TODO implement afterdeserialize properly
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

    // Stolen from MakeHistorical
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
                Db.l("Data loaded (Size in bytes: { " + bytes.Length + "})");
            }
            else
            {
                _data = new PropPainterDataContainer();

                Db.w("Data created");
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

            Db.l("Data saved (Size in bytes: { " + bytes.Length + "})");
        }
    }
}
