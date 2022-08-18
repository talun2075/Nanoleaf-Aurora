using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using Aurora;
using Newtonsoft.Json;

namespace AuroraWeb.Controllers
{
    public class EventController : ApiController
    {
        /// <summary>
        /// Counter der pro Session hoch zählt, damit evtl. Clients sich eine Liste geben können mit verpassten Nachrichten.
        /// </summary>
        private static int eventIDCounter = 0;
        /// <summary>
        /// Liste mit allen Events.
        /// </summary>
        private static Dictionary<int, AuroraLastChangeItem> EventList = new Dictionary<int, AuroraLastChangeItem>(); 
        /// <summary>
        /// Clients die nicht mehr verbunden sind
        /// </summary>
        static readonly List<StreamWriter> DisconnectedClients = new List<StreamWriter>();
        /// <summary>
        /// Hält alle Clients (Verbundene und alte)
        /// </summary>
        private static readonly List<StreamWriter> _streammessage = new List<StreamWriter>();
        /// <summary>
        /// Stream Connector um eine Subscription zu machen
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public HttpResponseMessage Get(HttpRequestMessage request)
        {
            try
            {
                HttpResponseMessage response = request.CreateResponse();
                response.Content = new PushStreamContent(OnStreamAvailable, "text/event-stream");
                response.Headers.CacheControl = new CacheControlHeaderValue
                {
                    Public = true,
                    MaxAge = new TimeSpan(0, 0, 0, 1)
                };
                return response;
            }
            catch
            {
                //ignore
                return null;
            }
        }
        /// <summary>
        /// Wird bei Allen Änderungen an einem Player aufgerufen
        /// </summary>
        /// <param name="pl"></param>
        public static void EventAuroraChange(AuroraConstants.AuroraEvents eventchange, AuroraLigth auro)
        {
            try
            {
                var t = new AuroraLastChangeItem
                {
                    Serial = auro.SerialNo,
                    LastChange = auro.LastChange,
                    TypeEnum = eventchange

                };
                switch (eventchange)
                {
                    case AuroraConstants.AuroraEvents.Brightness:
                        t.ChangedValues.Add(eventchange.ToString(), auro.Brightness.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.ColorMode:
                        t.ChangedValues.Add(eventchange.ToString(), auro.ColorMode);
                        break;
                    case AuroraConstants.AuroraEvents.ColorTemperature:
                        t.ChangedValues.Add(eventchange.ToString(), auro.ColorTemperature.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Hue:
                        t.ChangedValues.Add(eventchange.ToString(), auro.Hue.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Power:
                        t.ChangedValues.Add(eventchange.ToString(), auro.PowerOn.ToString().ToLower());
                        break;
                    case AuroraConstants.AuroraEvents.Saturation:
                        t.ChangedValues.Add(eventchange.ToString(), auro.Saturation.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Scenarios:
                        t.ChangedValues.Add(eventchange.ToString(), JsonConvert.SerializeObject(auro.Scenarios));
                        t.ChangedValues.Add("ScenariosDetailed", JsonConvert.SerializeObject(auro.NLJ.Effects.ScenariosDetailed));
                        break;
                    case AuroraConstants.AuroraEvents.NewNLJ:
                        t.ChangedValues.Add(eventchange.ToString(), JsonConvert.SerializeObject(auro.NLJ));
                        break;
                    case AuroraConstants.AuroraEvents.SelectedScenario:
                        t.ChangedValues.Add(eventchange.ToString(), auro.SelectedScenario);
                        break;
                    default:
                        t.ChangedValues.Add(eventchange.ToString(), "Unbekannter Wert");
                        break;
                }
                eventIDCounter++;
                t.ChangedValues.Add("EventID:", eventIDCounter.ToString());
                EventList.Add(eventIDCounter, t);
                DataFlush(t);
            }
            catch
            {
                //ignore
            }
        }
        public static void DataFlush(AuroraLastChangeItem t)
        {
            foreach (var data in _streammessage.ToArray())
            {
                try
                {
                    if (data == null)
                    {
                        lock (DisconnectedClients)
                        {
                            DisconnectedClients.Add(data);
                        }
                        continue;
                    };
                    data?.WriteLine("data:" + JsonConvert.SerializeObject(t) + "\n\n");
                    data?.Flush();
                    data?.Flush();
                }
                catch
                {
                    lock (DisconnectedClients)
                    {
                        DisconnectedClients.Add(data);
                    }
                }
            }
            if (DisconnectedClients.Count == 0) return;
            lock (DisconnectedClients)
            {
                foreach (StreamWriter disconnectedClient in DisconnectedClients)
                {
                    _streammessage.Remove(disconnectedClient);
                    disconnectedClient.Close();
                    disconnectedClient?.Dispose();
                }
                DisconnectedClients.Clear();
            }
        }
        private static void OnStreamAvailable(Stream stream, HttpContent headers, TransportContext context)
        {
            try
            {
                StreamWriter streamwriter = new StreamWriter(stream);
                if (!_streammessage.Contains(streamwriter))
                {
                    _streammessage.Add(streamwriter);
                }
            }
            catch
            {
                //ignore
            }
        }
        /// <summary>
        /// Liefert eine Liste mit allen Events nach der überlieferten id;
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static List<AuroraLastChangeItem> GetEventListbyID(int id)
        {
            var currenteventid = eventIDCounter;
            if (id > currenteventid || id < 1) return null;
            List<AuroraLastChangeItem> rinconLastChangeItems = new List<AuroraLastChangeItem>();
            for (int i = id; i < currenteventid+1; i++)
            {
                //Wert aus Dictionary auslesen und in liste legen
                if(EventList.TryGetValue(i, out AuroraLastChangeItem rlc))
                {
                    rinconLastChangeItems.Add(rlc);
                }
            }
            return rinconLastChangeItems;

        }
    }
    /// <summary>
    /// Element welches die letzten Änderungen bereit hält.
    /// </summary>
    public class AuroraLastChangeItem
    {
        /// <summary>
        /// Serial des betrefenden Lampe
        /// </summary>
        public String Serial { get; set; }
        /// <summary>
        /// Wann ist die Änderung passiert.
        /// </summary>
        public DateTime LastChange { get; set; }
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
