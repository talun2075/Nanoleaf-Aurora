using EvtSource;
using System.Text.Json;
using System;
using System.Threading.Tasks;
using AuroraCoreLib.Overrides;
using AuroraCoreLib.Enums;

namespace Aurora
{
    /// <summary>
    /// Used to Handle Events from Aurora and Convert it to Objects
    /// </summary>
    public class AuroraEvent
    {
        public event EventHandler<AuroraFiredEvent> Aurora_Subscriped_Event_Fired = delegate { };
        private readonly EventSourceReader evt;
        private AuroraEventConstructor aec;
        public AuroraEvent(AuroraEventConstructor _aec)
        {
            try
            {
                aec = _aec;
                //AuroraConstants.log.InfoLog("AuroraEvent:Create", aec.URI);
                if(evt != null)
                {
                    evt.Dispose();
                }

                evt = new EventSourceReader(new Uri(aec.URI));
                evt.MessageReceived += Evt_MessageReceived;
                evt.Disconnected += EventSourcReaderEvent;
                evt.Start();
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("AuroraEvent:Global", ex, aec.URI);
            }
        }

        private async void EventSourcReaderEvent(object sender, DisconnectEventArgs e)
        {
            await Task.Delay(e.ReconnectDelay);
            evt.Start();
        }
        private void Evt_MessageReceived(object sender, EventSourceMessageEventArgs e)
        {
            try
            {
                if (Enum.TryParse<EventIDTypes>(e.Id, out EventIDTypes eIT))
                {
                    if (eIT == EventIDTypes.State || eIT == EventIDTypes.Effects ||eIT == EventIDTypes.Touch && !string.IsNullOrEmpty(e.Message))
                    {
                        JsonSerializerOptions jsonSerializerOptions = new();
                        jsonSerializerOptions.Converters.Add(new StringConverter()); //Doku siehe im StringConverter
                        AuroraFiredEvent aFE = new();
                        aFE = JsonSerializer.Deserialize<AuroraFiredEvent>(e.Message,jsonSerializerOptions);
                        aFE.ID = eIT;
                        Aurora_Subscriped_Event_Fired(this, aFE);
                    }
                }
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("Evt_MessageReceived:Global", ex, aec.URI);
            }
        }

        public void Dispose()
        {
            evt.Disconnected -= EventSourcReaderEvent;
            evt.Dispose();
            aec = null;
        }
    }
}
