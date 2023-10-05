using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace AuroraCoreLib.DataClasses
{
    public class TouchSystemConfig
    {
        [JsonPropertyName("touchConfig")]
        public TouchConfig TouchConfig { get; set; }

    }
      public class DefaultSystemConfig
    {
        [JsonPropertyName("gestureActions")]
        public List<GestureAction> GestureActions { get; set; }
    }

    public class GestureAction
    {
        [JsonPropertyName("gesture")]
        public string Gesture { get; set; }//dt=DoubleTouch, sd=SwipeDown, sl = SwipeLeft, sr = SwipeRight

        [JsonPropertyName("action")]
        public object Action { get; set; }//null, pwr = Power On, bu = Brightness Up, bd = Brightness Down, ncs= Next Scenario, nrs = Next Rythm Scenario, nrdms = Next Random Scenario, pcs = previous Scenario, prs = previous Rhytm Scenario 
    }

    public class SupportedFeatures
    {
        [JsonPropertyName("systemGestures")]
        public List<string> SystemGestures { get; set; }

        [JsonPropertyName("systemActions")]
        public List<string> SystemActions { get; set; }
    }

    public class TouchConfig
    {
        [JsonPropertyName("supportedFeatures")]
        public SupportedFeatures SupportedFeatures { get; set; }

        [JsonPropertyName("defaultSystemConfig")]
        public DefaultSystemConfig DefaultSystemConfig { get; set; }

        [JsonPropertyName("userSystemConfig")]
        public UserSystemConfig UserSystemConfig { get; set; }

        [JsonPropertyName("userPanelConfigs")]
        public List<object> UserPanelConfigs { get; set; }
    }

    public class UserSystemConfig
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; } = false;

        [JsonPropertyName("gestureActions")]
        public List<GestureAction> GestureActions { get; set; }

        [JsonPropertyName("subscribers")]
        public List<object> Subscribers { get; set; }
    }


}
