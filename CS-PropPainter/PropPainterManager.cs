using System;
using ColossalFramework;
using System.Collections.Generic;

namespace PropPainter
{
    public class PropPainterManager
    {
        public static PropPainterManager instance;

        public Dictionary<ushort, SerializableColor> map = new Dictionary<ushort, SerializableColor>();
    }
}
