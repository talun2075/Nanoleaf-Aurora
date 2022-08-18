using HomeLogging;

namespace Aurora
{
    public static class AuroraConstants
    {
        public static Logging log = new Logging(new LoggerWrapperConfig { ErrorFileName = "AuroraErrors.txt", TraceFileName = "AuroraTrace.txt", InfoFileName = "AuroraInfo.txt", ConfigName = "Aurora", AddDateTimeToFilesNames = true });
        /// <summary>
        /// Event Enums
        /// </summary>
        public enum AuroraEvents
        {
            Power,
            SelectedScenario,
            Brightness,
            ColorMode,
            ColorTemperature,
            Hue,
            Saturation,
            Scenarios,
            NewNLJ
        }

        /// <summary>
        /// All Accepted Request Types. ConnectToNanoleaf only prepared for GET/PUT/POST
        /// </summary>
        internal enum RequestTypes
        {
            GET,
            PUT,
            POST,
        }
        public static int Port { get; private set; } = 16021;
        public static string Statepath { get; private set; } = "/state";
        public static string Effectspath { get; private set; } = "/effects";
        public static string Apipath { get; private set; } = "/api/v1/";
        /// <summary>
        /// Wird verwendet, wenn man Farben etc. Manuell setzen möchte
        /// </summary>
        public static string Solid { get; private set; } = "*Solid*";
        /// <summary>
        /// Wird verwendet, wenn man Farben etc. Manuell setzen möchte
        /// </summary>
        public static string ColorMode { get; private set; } = "hs";
        public static string RetvalPutPostOK { get; private set; } = "ok";
    }


}
