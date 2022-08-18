using System;
using System.Collections.Generic;

namespace Aurora
{
    /// <summary>
    /// To Subscripe to Light Events we need this COnsturctor to know wicht Event Types and URI we want to Subsripe
    /// Supported State and Effects for this Moment
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
                List<String> usedids = new List<string>();
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
        public Boolean EventTypeTouch { get; private set; } = false;

    }
    /// <summary>
    /// Event fired from Aurora Device
    /// </summary>
    public class AuroraFiredEvent
    {
        public EventIDTypes ID { get; set; }
        public List<AuroraFiredEventValue> events { get; set; } = new List<AuroraFiredEventValue>();

    }
    /// <summary>
    /// The ValueTypes of fired Events by Device
    /// Only Support for State and Effects EventIDs
    /// </summary>
    public class AuroraFiredEventValue
    {
        public string attr { get; set; }
        public string value { get; set; }
    }
    /// <summary>
    /// Event Types to Subscripe
    /// </summary>
    public enum EventIDTypes
    {
        State = 1,
        Layout = 2,
        Effects = 3,
        Touch = 4
    }
    /// <summary>
    /// Event Attributes for State Events
    /// </summary>
    public enum EventIDStateAttributtes
    {
        on = 1,
        brightness = 2,
        hue = 3,
        saturation = 4,
        cct = 5,
        colorMode = 6
    }
    /// <summary>
    /// Event Attributes for Layout Events
    /// </summary>
    public enum EventIDLayoutAttributtes
    {
        layout = 1,
        globalOrientation = 2
    }
    /// <summary>
    /// Event Attributes for Effects Events
    /// </summary>
    public enum EventIDEffectsAttributtes
    {
        EffectName = 1
    }
    /// <summary>
    /// Event Attributes for Touch Events
    /// </summary>
    public enum EventIDTouchAttributtes
    {
        SingleTap = 1,
        DoubleTap = 2,
        SwipeUp = 3,
        SwipeDown = 4,
        SwipeLeft = 5,
        SwipeRight = 6
    }
}
