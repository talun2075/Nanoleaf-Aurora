using EvtSource;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace Aurora
{
    public class AuroraEvent
    {
        public event EventHandler<AuroraFiredEvent> Aurora_Subscriped_Event_Fired = delegate { };
        private EventSourceReader evt;
        private AuroraEventConstructor aec;
        public AuroraEvent(AuroraEventConstructor _aec)
        {
            try
            {
                aec = _aec;
                AuroraConstants.log.InfoLog("AuroraEvent:Create", aec.URI);
                evt = new EventSourceReader(new Uri(aec.URI)).Start();
                evt.MessageReceived += Evt_MessageReceived;
                evt.Disconnected += async (object sender, DisconnectEventArgs e) =>
                {
                    if (e.Exception != null)
                        AuroraConstants.log.ServerErrorsAdd("AuroraEvent:Disconnected", e.Exception);
                    await Task.Delay(e.ReconnectDelay);
                    evt.Start(); // Reconnect to the same URL
                };
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("AuroraEvent:Global", ex, aec.URI);
            }
        }
        private void Evt_MessageReceived(object sender, EventSourceMessageEventArgs e)
        {
            try
            {
                if (Enum.TryParse<EventIDTypes>(e.Id, out EventIDTypes eIT))
                {
                    if (eIT == EventIDTypes.State || eIT == EventIDTypes.Effects)
                    {
                        AuroraFiredEvent aFE = JsonConvert.DeserializeObject<AuroraFiredEvent>(e.Message);
                        aFE.ID = eIT;
                        AuroraConstants.log.InfoLog("AuroraEvent:Evt_MessageReceived", e.Message);
                        Aurora_Subscriped_Event_Fired(this, aFE);
                    }
                }
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("Evt_MessageReceived:Global", ex, aec.URI);
            }
        }

    }
}
