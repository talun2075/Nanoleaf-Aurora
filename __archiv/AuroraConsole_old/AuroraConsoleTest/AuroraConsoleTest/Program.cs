using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using EvtSource;
using Aurora;

namespace AuroraConsoleTest
{
    class Program
    {
        private static HttpClient _httpClient = new HttpClient();
        static string UriOfEvent = "http://192.168.0.110:16021/api/v1/p7rY1vD2YxRQLkLZ8SxhYtVsIhCTMsp3/events?id=1,3";
        static void Main(string[] args)
        {
            try
            {
                //https://github.com/3ventic/EvtSource
                //testEvent();
                //TextEffectCall();
                TestDNS();
                Console.ReadLine();
                return;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }
        static void TestDNS()
        {
            try
            {
                string url = "http://sonos.fritz.box/sonos/devices/get";
                HttpResponseMessage result;
                string returnValue;
                result = _httpClient.GetAsync(url).Result;
                returnValue = result.Content.ReadAsStringAsync().Result;
            }catch(Exception ex)
            {
                var k = ex.Message;
            }
        }
        static async void TestAuroraAssemly()
        {
           await AuroraWrapper.InitAuroraWrapper();
            Console.WriteLine("init durch");
        }
        static void testEvent()
        {
            try
            {
                var evt = new EventSourceReader(new Uri(UriOfEvent)).Start();
                evt.MessageReceived += Evt_MessageReceived;
                evt.Disconnected += async (object sender, DisconnectEventArgs e) => {
                    Console.WriteLine($"Retry: {e.ReconnectDelay} - Error: {e.Exception.Message}");
                    await Task.Delay(e.ReconnectDelay);
                    evt.Start(); // Reconnect to the same URL
                };
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private static void Evt_MessageReceived(object sender, EventSourceMessageEventArgs e)
        {
            var xx = JsonConvert.DeserializeObject<auroraevent>(e.Message);
            xx.ID = e.Id;
            Console.WriteLine($"{e.Event} : {e.Message}");

            //{"events":[{"attr":4,"value":74},{"attr":6,"value":"hs"}]}
            //{ "events":[{"attr":1,"value":"*Solid*"}]}

        }

        static void TextStateCall()
        {

            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            Boolean power = false;
            string url = "http://192.168.0.110:16021/api/v1/p7rY1vD2YxRQLkLZ8SxhYtVsIhCTMsp3/state/";
            //string url = "https://httpbin.org/put";
            HttpResponseMessage result;
            string returnValue;
            result = _httpClient.GetAsync(url).Result;
            returnValue = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine(returnValue);
            Console.WriteLine("---------------------------------------------");

            string json = "{\"on\":{\"value\":" + power.ToString().ToLower() + "}}";
            using (var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"))
            {
                result = _httpClient.PutAsync(url, content).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.Created)
                    Console.WriteLine("ok");
                returnValue = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(returnValue);
            }
        }
        static void TextEffectCall()
        {

            _httpClient.DefaultRequestHeaders.ExpectContinue = false;
            string url = "http://192.168.0.110:16021/api/v1/p7rY1vD2YxRQLkLZ8SxhYtVsIhCTMsp3/effects/";
            //string url = "https://httpbin.org/put";
            HttpResponseMessage result;
            string returnValue;
            result = _httpClient.GetAsync(url).Result;
            returnValue = result.Content.ReadAsStringAsync().Result;
            Console.WriteLine(returnValue);
            Console.WriteLine("---------------------------------------------");

            string json = "{\"write\":{\"command\":\"requestAll\"}}";
            using (var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json"))
            {
                result = _httpClient.PutAsync(url, content).Result;
                if (result.StatusCode == System.Net.HttpStatusCode.Created)
                    Console.WriteLine("ok");
                returnValue = result.Content.ReadAsStringAsync().Result;
                Console.WriteLine(returnValue);
            }
        }
    }

    public class auroraevent
    {
        public string ID { get; set; }
        public List<auroaeventvalue> events { get; set; } = new List<auroaeventvalue>();

    }
    public class auroaeventvalue
    {
        public int attr { get; set; }
        public string value { get; set; }
    }

}
