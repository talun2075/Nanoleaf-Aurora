#nullable enable
using Aurora;
using AuroraCore.Classes.Events;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;


namespace AuroraWeb.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EventController : ControllerBase
    {
        private static readonly IMessageRepository _messageRepository = new MessageRepository();

        private static readonly JsonSerializerOptions _jsonSerializerOptions = new ()
        { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        /// <summary>
        /// Produce SSE
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [Produces("text/event-stream")]
        [HttpGet]
        public async Task SubscribeEvents(CancellationToken cancellationToken)
        {
            SetServerSentEventHeaders();
            // On connect, welcome message ;)
            var data = new { Message = "connected!" };
            var jsonConnection = JsonSerializer.Serialize(data, _jsonSerializerOptions);
            await Response.WriteAsync($"event:connection\n", cancellationToken);
            await Response.WriteAsync($"data: {jsonConnection}\n\n", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
            async void OnNotification(object? sender, NotificationArgs eventArgs)
            {
                try
                {
                    // idea: https://stackoverflow.com/a/58565850/80527
                    var json = PrepareData(eventArgs);
                    //await Response.WriteAsync($"id:{eventArgs.Notification.Aurora.SerialNo}\n", cancellationToken);
                    //await Response.WriteAsync("retry: 10000\n", cancellationToken);
                    await Response.WriteAsync($"event:aurora\n", cancellationToken);
                    await Response.WriteAsync($"data:{json}\n\n", cancellationToken);
                    await Response.Body.FlushAsync(cancellationToken);
                }
                catch (Exception ex)
                {
                    var k = ex.Message;
                }
            }
            _messageRepository.NotificationEvent += OnNotification;

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    // Spin until something break or stop...
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {

            }
            finally
            {
                _messageRepository.NotificationEvent -= OnNotification;
            }
        }
        private static string PrepareData(NotificationArgs eventArgs)
        {
            try
            {
                var auro = eventArgs.Notification.Aurora;
                var eventchange = eventArgs.Notification.EventType;
                var t = new AuroraLastChangeItem
                {
                    Serial = auro.SerialNo,
                    TypeEnum = eventchange

                };

                switch (eventchange)
                {
                    case AuroraConstants.AuroraEvents.Brightness:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.Brightness.Value.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.ColorMode:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.ColorMode);
                        break;
                    case AuroraConstants.AuroraEvents.ColorTemperature:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.ColorTemperature.Value.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Hue:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.Hue.Value.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Power:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.Powerstate.Value.ToString().ToLower());
                        break;
                    case AuroraConstants.AuroraEvents.Saturation:
                        t.ChangedValues.Add(eventchange.ToString(), auro.NLJ.State.Saturation.Value.ToString());
                        break;
                    case AuroraConstants.AuroraEvents.Scenarios:
                        t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(auro.Scenarios, _jsonSerializerOptions));
                        t.ChangedValues.Add("ScenariosDetailed", JsonSerializer.Serialize(auro.NLJ.Effects.ScenariosDetailed, _jsonSerializerOptions));
                        break;
                    case AuroraConstants.AuroraEvents.NewNLJ:
                        t.ChangedValues.Add(eventchange.ToString(), JsonSerializer.Serialize(auro.NLJ, _jsonSerializerOptions));
                        break;
                    case AuroraConstants.AuroraEvents.SelectedScenario:
                        t.ChangedValues.Add(eventchange.ToString(), auro.SelectedScenario);
                        break;
                    default:
                        t.ChangedValues.Add(eventchange.ToString(), "Unbekannter Wert");
                        break;
                }
                //eventIDCounter++;
                //t.ChangedValues.Add("EventID:", eventIDCounter.ToString());
                //EventList.Add(eventIDCounter, t);
                //DataFlush(t);
                return JsonSerializer.Serialize(t, _jsonSerializerOptions);
            }
            catch
            {
                //ignore
                return String.Empty;
            }
        }
        public static Task EventBroadCast(Notification notification)
        {
            _messageRepository.Broadcast(notification);

            return Task.CompletedTask;
        }

        private void SetServerSentEventHeaders()
        {
            Response.StatusCode = 200;
            Response.Headers.Add("Content-Type", "text/event-stream");
            Response.Headers.Add("Cache-Control", "no-cache");
            Response.Headers.Add("Connection", "keep-alive");
        }
        [HttpPost("broadcast")]
        public Task Broadcast([FromBody] Notification notification)
        {
            _messageRepository.Broadcast(notification);

            return Task.CompletedTask;
        }
    }
}
