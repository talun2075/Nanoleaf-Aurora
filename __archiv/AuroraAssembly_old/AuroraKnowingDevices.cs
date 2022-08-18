using System;

namespace Aurora
{
    /// <summary>
    /// Class for your Knowing Devices.
    /// </summary>
    public class AuroraKnowingDevices
    {
        public AuroraKnowingDevices() { }

        public AuroraKnowingDevices(String _MacAdress, String _AuthToken, String _DeviceName, Boolean _useSubscription = false)
        {
            MacAdress = _MacAdress;
            AuthToken = _AuthToken;
            DeviceName = _DeviceName;
            UseSubscription = _useSubscription;
        }
        public AuroraKnowingDevices(String _MacAdress, String _AuthToken, String _DeviceName, String IP, Boolean _useSubscription = false)
        {
            MacAdress = _MacAdress;
            AuthToken = _AuthToken;
            DeviceName = _DeviceName;
            KnowingIP = IP;
            UseSubscription = _useSubscription;
        }
        public AuroraKnowingDevices(String _MacAdress, String _AuthToken, String _DeviceName, String IP, String serial, Boolean _useSubscription = false)
        {
            MacAdress = _MacAdress;
            AuthToken = _AuthToken;
            DeviceName = _DeviceName;
            KnowingIP = IP;
            Serial = serial;
            UseSubscription = _useSubscription;
        }
        /// <summary>
        /// Knowing Mac Adress
        /// </summary>
        public String MacAdress { get; set; }
        /// <summary>
        /// Used AuthToken
        /// </summary>
        public String AuthToken { get; set; }
        /// <summary>
        /// Internal Name we Use
        /// </summary>
        public String DeviceName { get; set; }
        /// <summary>
        /// Internal IP we Use
        /// </summary>
        public String KnowingIP { get; set; }
        /// <summary>
        /// Knowing Serial of the Device
        /// </summary>
        public String Serial { get; set; }
        /// <summary>
        /// Should we use SSE (Server Sent Event from Aurora to Server)
        /// </summary>
        public Boolean UseSubscription { get; private set; } = false;
    }
}
