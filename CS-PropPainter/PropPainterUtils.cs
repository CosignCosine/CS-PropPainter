using System;
using UnityEngine;

namespace PropPainter
{
    [Serializable]
    public class SerializableColor {
        public float r;
        public float g;
        public float b;
        public float a;

        public SerializableColor(){
            r = 0;
            g = 0;
            b = 0;
            a = 1;
        }

        public SerializableColor(float _r, float _g, float _b){
            r = _r;
            g = _g;
            b = _b;
            a = 1;
        }

        public SerializableColor(float _r, float _g, float _b, float _a)
        {
            r = _r;
            g = _g;
            b = _b;
            a = _a;
        }

        public static implicit operator SerializableColor(Color t)
        {
            return new SerializableColor(t.r, t.g, t.b, t.a);
        }

        public static implicit operator Color(SerializableColor t)
        {
            return new Color(t.r, t.g, t.b, t.a);
        }
    }
}
