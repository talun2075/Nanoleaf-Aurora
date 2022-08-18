using AuroraCoreLib.Enums;
using System;

namespace AuroraCoreLib.DataClasses
{
    public class TouchData
    {
        public int Hue { get; set; } = -1;
        public int Saturation { get; set; } = -1;

        public int Brightness { get; set; } = -1;

        public String Value { get; set; }

        public TouchEventActions EventActions { get; set; }

        public EventIDTouchAttributtes EventType { get; set; }
    }
}
