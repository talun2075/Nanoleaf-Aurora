using AuroraCoreLib.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aurora
{
    #region NanoleafJsonTranslate

    [DataContract]
    public class NanoLeafJson
    {
        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "serialNo")]
        public string SerialNo { get; set; }

        [DataMember(Name = "manufacturer")]
        public string Manufacturer { get; set; }

        [DataMember(Name = "firmwareVersion")]
        public string FirmwareVersion { get; set; }

        [DataMember(Name = "model")]
        public string Model { get; set; }

        [DataMember(Name = "cloudHash")]
        public string CloudHash { get; set; }

        [DataMember(Name = "discovery")]
        public string Discovery { get; set; }

        [DataMember(Name = "schedules")]
        public string Schedules { get; set; }

        [DataMember(Name = "state")]
        public NanoleafJsonState State { get; set; }

        [DataMember(Name = "effects")]
        public NanoleafJsonEffects Effects { get; set; }

        [DataMember(Name = "panelLayout")]
        public NanoleafJsonPanelLayout PanelLayout { get; set; }

        [DataMember(Name = "rhythm")]
        public NanoLeafJsonRhythm Rhythm { get; set; }
    }

    /// <summary>
    /// Abstract State from Json
    /// </summary>
    [DataContract]
    public class NanoleafJsonState
    {
        [DataMember(Name = "on")]
        public NanoLeafJsonValue Powerstate { get; set; }

        [DataMember(Name = "colorMode")]
        public string ColorMode { get; set; }

        [DataMember(Name = "brightness")]
        public NanoleafJsonVMM Brightness { get; set; }

        [DataMember(Name = "hue")]
        public NanoleafJsonVMM Hue { get; set; }

        [DataMember(Name = "sat")]
        public NanoleafJsonVMM Saturation { get; set; }

        [DataMember(Name = "ct")]
        public NanoleafJsonVMM ColorTemperature { get; set; }
    }

    /// <summary>
    /// Abstract Value/Max/Min Object from Json
    /// </summary>
    [DataContract]
    public class NanoleafJsonVMM
    {
        [DataMember(Name = "value")]
        public int Value { get; set; }

        [DataMember(Name = "max")]
        public int Max { get; set; }

        [DataMember(Name = "min")]
        public int Min { get; set; }
    }

    [DataContract]
    public class NanoleafJsonEffects
    {
        [DataMember(Name = "select")]
        public string Selected { get; set; }

        [DataMember(Name = "effectsList")]
        public List<String> Scenarios { get; set; }
        [DataMember(Name = "effectsListDetailed")]
        public NanoLeafJsonDetailedEffectListAnimationRoot ScenariosDetailed { get; set; }
    }

    [DataContract]
    public class NanoleafJsonPanelLayout
    {
        [DataMember(Name = "globalOrientation")]
        public NanoleafJsonVMM GlobalOrientation { get; set; }

        [DataMember(Name = "layout")]
        public NanoleafJsonPanelLayoutLayout Layout { get; set; }
    }

    [DataContract]
    public class NanoleafJsonPanelLayoutLayout
    {
        [DataMember(Name = "numPanels")]
        public int NumPanels { get; set; }
        
        [DataMember(Name = "sideLength")]
        public int SideLength { get; set; }
        
        [DataMember(Name = "positionData")]
        public List<NanoLeafJsonPositionData> PositionData { get; set; }
    }

    [DataContract]
    public class NanoLeafJsonPositionData
    {
        [DataMember(Name = "panelId")]
        public int PanelId { get; set; }
        
        [DataMember(Name = "x")]
        public int X { get; set; }

        [DataMember(Name = "y")]
        public int Y { get; set; }

        [DataMember(Name = "o")]
        public int Orientation { get; set; }

        [DataMember(Name = "shapeType")]
        public ShapeType ShapeType { get; set; }

        public int SideLenght
        {
            get
            {
                int lengt = 0;
                switch (ShapeType)
                {
                    case ShapeType.ControlSquareMaster:
                    case ShapeType.ControlSquarePassive:
                    case ShapeType.Square:
                        lengt = 100;
                        break;
                    case ShapeType.Triangle:
                        lengt = 150;
                        break;
                    case ShapeType.ControllerCap:
                    case ShapeType.PowerConnector:
                    case ShapeType.LinesConnector:
                        lengt = 11;
                        break;
                    case ShapeType.HexagonShapes:
                    case ShapeType.MiniTriangleShapes:
                        lengt = 67;
                        break;
                    case ShapeType.TriangleShapes:
                    case ShapeType.ElementsHexagons:
                        lengt = 134;
                        break;
                    case ShapeType.LightLines:
                        lengt = 154;
                        break;
                    case ShapeType.LightLinesSingleZone:
                        lengt = 77;
                        break;
                    case ShapeType.ElementsHexagonsCorner:
                        lengt = 58;
                        break;
                }
                return lengt;
            }
        }
    }

    [DataContract]
    public class NanoLeafJsonValue
    {
        [DataMember(Name = "value")]
        public Boolean Value { get; set; }
    }

    [DataContract]
    public class NanoLeafJsonRhythm
    {
        [DataMember(Name = "rhythmConnected")]
        public Boolean RhythmConnected { get; set; }

        [DataMember(Name = "rhythmActive")]
        public Boolean? RhythmActive { get; set; }

        [DataMember(Name = "rhythmId")]
        public String RhythmId { get; set; }

        [DataMember(Name = "hardwareVersion")]
        public String HardwareVersion { get; set; }

        [DataMember(Name = "firmwareVersion")]
        public String FirmwareVersion { get; set; }

        [DataMember(Name = "auxAvailable")]
        public Boolean? AuxAvailable { get; set; }

        [DataMember(Name = "rhythmMode")]
        public String RhythmMode { get; set; }

        [DataMember(Name = "rhythmPos")]
        public NanoLeafJsonRhythmPos RhythmPos { get; set; }
    }

    [DataContract]
    public class NanoLeafJsonRhythmPos
    {
        [DataMember(Name = "x")]
        public String x { get; set; }

        [DataMember(Name = "y")]
        public String y { get; set; }

        [DataMember(Name = "o")]
        public String o { get; set; }
    }

    [DataContract]
    public class NanoLeafJsonDetailedEffectListAnimationRoot
    {
        [DataMember(Name = "animations")]
        public List<NanoLeafJsonDetailedEffectListEntry> Animations { get; set; }
    }
    [DataContract]
    public class NanoLeafJsonDetailedEffectListEntry
    {
        [DataMember(Name = "version")]
        public String Version { get; set; } = "2.0";

        [DataMember(Name = "animName")]
        public String AnimName { get; set; } = String.Empty;

        [DataMember(Name = "animType")]
        public String AnimType { get; set; } = String.Empty;

        [DataMember(Name = "colorType")]
        public String ColorType { get; set; } = "HSB";

        [DataMember(Name = "pluginType")]
        public String PluginType { get; set; } = String.Empty;

        [DataMember(Name = "pluginUuid")]
        public String PluginUuid { get; set; } = String.Empty;

        [DataMember(Name = "hasOverlay")]
        public Boolean HasOverlay { get; set; } = false;

        [DataMember(Name = "palette")]
        public List<NanoLeafJsonDetailedEffectListEntryPalette> Palette { get; set; } = new List<NanoLeafJsonDetailedEffectListEntryPalette>();

        [DataMember(Name = "pluginOptions")]
        public List<NanoLeafJsonDetailedEffectListEntryPuginOptions> PluginOptions { get; set; }
    }
    [DataContract]
    public class NanoLeafJsonDetailedEffectListEntryPalette
    {
        [DataMember(Name = "hue")]
        public int Hue { get; set; } = 0;

        [DataMember(Name = "saturation")]
        public int Saturation { get; set; } = 0;

        [DataMember(Name = "brightness")]
        public int Brightness { get; set; } = 0;

        [DataMember(Name = "probability")]
        public String Probability { get; set; } = String.Empty;
    }
    [DataContract]
    public class NanoLeafJsonDetailedEffectListEntryPuginOptions
    {
        [DataMember(Name = "name")]
        public String Name { get; set; } = String.Empty;

        [DataMember(Name = "value")]
        public String Value { get; set; } = String.Empty;


    }
    #endregion NanoleafJsonTranslate
}
