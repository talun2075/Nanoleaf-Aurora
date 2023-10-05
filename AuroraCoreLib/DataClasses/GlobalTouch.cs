using System.Text.Json.Serialization;


namespace AuroraCoreLib.DataClasses
{
    internal class GlobalTouch
    {
        [JsonPropertyName("touchKillSwitchOn")]
        public bool TouchKillSwitchOn { get; set; } = false;
    }

}
