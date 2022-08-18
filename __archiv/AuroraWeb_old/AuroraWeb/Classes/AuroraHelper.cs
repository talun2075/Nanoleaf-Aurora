using HomeLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AuroraWeb.Classes
{
    public static class AuroraHelper
    {
        public static readonly Logging log = new Logging(new LoggerWrapperConfig { ErrorFileName = "AuroraErrorsWeb.txt", TraceFileName = "AuroraTraceWeb.txt", InfoFileName = "AuroraInfoWeb.txt", ConfigName = "AuroraWeb", AddDateTimeToFilesNames = true });
    
    }
}