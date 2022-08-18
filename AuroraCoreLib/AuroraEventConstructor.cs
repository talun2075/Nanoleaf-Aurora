using AuroraCoreLib.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Aurora
{
    /// <summary>
    /// To Subscripe to Light Events we need this Consturctor to know wicht Event Types and URI we want to Subsripe
    /// Supported State, Effects and Touch for this Moment
    /// </summary>
    public class AuroraEventConstructor
    {
        private string _uri;
        public AuroraEventConstructor(string uri)
        {
            _uri = uri;
        }
        /// <summary>
        /// Ground URI in Style http://<IP>/<Token>/
        /// </summary>
        public string URI
        {
            get
            {
                if (!EventTypeEffects && !EventTypeLayout && !EventTypeSate && !EventTypeTouch) return String.Empty;
                string e = "/events?id=";
                List<String> usedids = new();
                if (EventTypeEffects)
                    usedids.Add(((int)EventIDTypes.Effects).ToString());
                if (EventTypeLayout)
                    usedids.Add(((int)EventIDTypes.Layout).ToString());
                if (EventTypeSate)
                    usedids.Add(((int)EventIDTypes.State).ToString());
                if (EventTypeTouch)
                    usedids.Add(((int)EventIDTypes.Touch).ToString());
                if (usedids.Count == 1)
                {
                    e += usedids[0];
                }
                else
                {
                    for (int i = 0; i < usedids.Count; i++)
                    {
                        if (i == 0)
                        {
                            e += usedids[i];
                        }
                        else
                        {
                            e += "," + usedids[i];
                        }
                    }
                }

                return _uri + e;

            }

            set
            {
                _uri = value;
            }
        }
        /// <summary>
        /// Do we want the State Events?
        /// </summary>
        public Boolean EventTypeSate { get; set; } = true;
        /// <summary>
        /// Do we want the Layout Events?
        /// </summary>
        public Boolean EventTypeLayout { get; private set; } = false;
        /// <summary>
        /// Do we want the Effects Events?
        /// </summary>
        public Boolean EventTypeEffects { get; set; } = true;
        /// <summary>
        /// Do we want the Touch Events?
        /// </summary>
        public Boolean EventTypeTouch { get; private set; } = true;

    }
    /// <summary>
    /// Event fired from Aurora Device
    /// </summary>
    [DataContract]
    public class AuroraFiredEvent
    {
        [DataMember(Name = "id")]
        public EventIDTypes ID { get; set; }
        [DataMember(Name = "events")]
        public List<AuroraFiredEventValue> events { get; set; } = new List<AuroraFiredEventValue>();

    }
    /// <summary>
    /// The ValueTypes of fired Events by Device
    /// Only Support for State and Effects EventIDs
    /// </summary>
    [DataContract]
    public class AuroraFiredEventValue
    {
        [DataMember(Name = "attr")]
        public int attr { get; set; }
        [DataMember(Name = "value")]
        public string value { get; set; }
        [DataMember(Name = "panelId")]
        public int panelId { get; set; }
        [DataMember(Name = "gesture")]
        public int gesture { get; set; }

    }

}
