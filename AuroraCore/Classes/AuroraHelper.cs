using HomeLogging;

namespace AuroraWeb.Classes
{
    public static class AuroraHelper
    {
        public static readonly Logging log = new (new LoggerWrapperConfig { ErrorFileName = "AuroraErrorsWeb.txt", TraceFileName = "AuroraTraceWeb.txt", InfoFileName = "AuroraInfoWeb.txt", ConfigName = "AuroraWeb", AddDateTimeToFilesNames = true });

    }
}