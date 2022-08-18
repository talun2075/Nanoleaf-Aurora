using Aurora;
using System;
using System.Collections.Generic;

namespace AuroraCore.Classes.Events
{
    public class AuroraLastChangeItem
    {
        /// <summary>
        /// Serial des betrefenden Lampe
        /// </summary>
        public String Serial { get; set; }
        /// <summary>
        /// TypeEnum als String
        /// </summary>
        public String ChangeType => TypeEnum.ToString();
        /// <summary>
        /// Welches Event wurde ausgelöst
        /// </summary>
        internal AuroraConstants.AuroraEvents TypeEnum { get; set; }
        /// <summary>
        /// Welche Daten sollen mit gegeben werden. 
        /// Event ID und der geänderte Wert ist normal.
        /// </summary>
        public Dictionary<String, String> ChangedValues { get; set; } = new Dictionary<string, string>();
    }
}
