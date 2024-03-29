﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aurora
{
    /// <summary>
    /// Class to Communicate with a Nanoleaf Aurora
    /// </summary>
    [DataContract]
    public class AuroraLigth
    {
        public event EventHandler<AuroraLigth> Aurora_Changed = delegate { };
        private AuroraEvent _auroraEvent;
        /// <summary>
        /// Benutzter Web Client
        /// Wichtig: _httpClient.DefaultRequestHeaders.ExpectContinue = false; um mit StatusCode 204 arbeiten zu können.
        /// </summary>
        private HttpClient _httpClient;
        /// <summary>
        /// Init the aurora
        /// </summary>
        /// <param name="token">User Token type "New" for new User</param>
        /// <param name="_ip">IP of the Aurora</param>
        /// <param name="_Name"></param>
        /// <param name="port">Port (Default 16021)</param>
        public AuroraLigth(string token, string _ip, string _Name, string serial = "", Boolean SubscripeToDeviceEvents = false)
        {
            try
            {
                if (String.IsNullOrEmpty(_ip) || String.IsNullOrEmpty(token))
                    throw new ArgumentNullException(nameof(_ip), "ip or token is Empty");
                if (_ip.StartsWith("http://"))
                    _ip = _ip.Replace("http://", "");
                if (!Regex.IsMatch(_ip, @"^[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}$"))
                    throw new ArgumentOutOfRangeException(nameof(_ip), _ip, "This is not a IP");
                Token = token;
                Ip = _ip;
                Name = _Name;
                SerialNo = serial;
                //Wichtig. Wird für PUT und POST benötigt, wenn ein No Content Status (204) geliefert wird.
                _httpClient = new HttpClient();
                _httpClient.DefaultRequestHeaders.ExpectContinue = false;
                _httpClient.Timeout = new TimeSpan(0, 0, 12);
                if (SubscripeToDeviceEvents && _auroraEvent == null)
                {
                    _auroraEvent = new AuroraEvent(new AuroraEventConstructor("http://" + Ip + ":" + AuroraConstants.Port + AuroraConstants.Apipath + Token));
                    _auroraEvent.Aurora_Subscriped_Event_Fired += _auroraEvent_Aurora_Subscriped_Event_Fired;
                }
                if (Token.ToLower() == "new")
                {
                    NewAurora = true;
                }
            }
            catch (Exception ex)
            {

                AuroraConstants.log.ServerErrorsAdd("AuroraConstruktor", ex);
            }
        }
        #region PublicMethods
        public async Task<string> SetSaturation(int newvalue)
        {
            try
            {
                if (newvalue > NLJ.State.Saturation.Max || newvalue < NLJ.State.Saturation.Min)
                {
                    return "Saturation value out of Range";
                }
                var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, AuroraConstants.Statepath, "{\"sat\":{\"value\":" + newvalue + "}}");
                if (retval == AuroraConstants.RetvalPutPostOK)
                {
                    Saturation = newvalue;
                    ManuellStateChange(AuroraConstants.AuroraEvents.Saturation, DateTime.Now);
                    if (!PowerOn)
                    {
                        PowerOn = true;
                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetSaturation", ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// Use to Get a New Token
        /// Push the On Button for 5 till 7 Seconds on the Aurora and then Call this Method. You have 30 Seconds time.
        /// </summary>
        /// <returns>New Token</returns>
        public async Task<String> NewUser()
        {
            try
            {
                return await ConnectToNanoleaf(AuroraConstants.RequestTypes.POST, "new");
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("New User", ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// Setzt ein zufälliges Scenario an
        /// </summary>
        /// <param name="withRhythmEffects">Sollen Rhythm Modul Scenarien mit einbezogen werden?</param>
        /// <returns></returns>
        public async Task<String> SetRandomScenario(bool withRhythmEffects = false)
        {
            try
            {
                string animName = String.Empty;
                Random rng = new Random();
                int k = -1;
                if (withRhythmEffects)
                {
                    k = rng.Next(0, NLJ.Effects.ScenariosDetailed.Animations.Count - 1);
                    if (k > -1)
                        animName = NLJ.Effects.ScenariosDetailed.Animations[k].AnimName;
                }
                else
                {
                    var filteredscen = NLJ.Effects.ScenariosDetailed.Animations.Where(x => x.PluginType != "rhythm").ToList();
                    k = rng.Next(0, filteredscen.Count - 1);
                    if (k > -1)
                        animName = filteredscen[k].AnimName;
                }
                if (string.IsNullOrEmpty(animName)) return "Error beim ermitteln des ZufallsEffects";
                await SetSelectedScenario(animName);
                return NLJ.Effects.Selected;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetRandomScenario", ex);
                return String.Empty;
            }
        }
        /// <summary>
        /// Rename Scenario
        /// </summary>
        /// <param name="oldScenario">Name of Old Scenario. Must be in the EffectList (Scenarios)</param>
        /// <param name="newScenario">New Name</param>
        /// <returns>True if done or false on an Error</returns>
        public async Task<Boolean> RenameScenario(string oldScenario, string newScenario)
        {
            if (!Scenarios.Contains(oldScenario) || string.IsNullOrEmpty(newScenario)) return false;
            try
            {
                string jsontemp = "{\"write\" : {\"command\" : \"rename\", \"animName\" : \"" + oldScenario +
                                  "\",\"newName\" : \"" + newScenario + "\"}}";
                var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, "/effects", jsontemp);
                if (retval == AuroraConstants.RetvalPutPostOK)
                {
                    var k = Scenarios.FirstOrDefault(x => x == oldScenario);
                    if (k != null)
                        k = newScenario;
                }
                await GetNanoLeafInformations();
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("RenameScenario", ex);
                return false;
            }
            return true;
        }
        /// <summary>
        /// Set one Color to Aurora
        /// </summary>
        /// <param name="hue">Hue</param>
        /// <param name="saturation">Saturation</param>
        /// <param name="brightness">Brightness</param>
        /// <returns></returns>
        public async Task<Boolean> SetHSV(int hue, int saturation, int brightness)
        {
            try
            {
                if (hue > NLJ.State.Hue.Max || hue < NLJ.State.Hue.Min)
                {
                    return false;
                }
                await SetHue(hue);
                if (saturation > NLJ.State.Saturation.Max || saturation < NLJ.State.Saturation.Min)
                {
                    return false;
                }
                await SetSaturation(saturation);
                if (brightness > NLJ.State.Brightness.Max || brightness < NLJ.State.Brightness.Min)
                {
                    return false;
                }
                await SetBrightness(brightness);
                return true;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetHSV", ex);
                return false;
            }
        }
        /// <summary>
        /// Class to get Changes from Nanoleaf
        /// On Each Change its to call, that the Changes Knowing
        /// </summary>
        /// <returns></returns>
        public async Task<Boolean> GetNanoLeafInformations()
        {
            var json = await ConnectToNanoleaf(AuroraConstants.RequestTypes.GET, "INIT");

            if (String.IsNullOrEmpty(json))
            {
                return false;
            }
            try
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    // Deserialization from JSON  
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(NanoLeafJson));
                    NLJ = (NanoLeafJson)deserializer.ReadObject(ms);
                    if (NLJ != null)
                        SerialNo = NLJ.SerialNo;
                }
                await GetNanoleafDetailedEffectList();
                return true;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("GetNanoLeafInformations", ex);
                return false;
            }
        }
        public async Task<Boolean> RefreshProperties()
        {
            //Json holen und auf PropertieEbene vergleichen.
            var json = await ConnectToNanoleaf(AuroraConstants.RequestTypes.GET, "INIT");
            var nl = new NanoLeafJson();
            if (String.IsNullOrEmpty(json))
            {
                return false;
            }
            try
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    // Deserialization from JSON  
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(NanoLeafJson));
                    nl = (NanoLeafJson)deserializer.ReadObject(ms);

                }
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("RefreshProperties:Block NanoLeafJson", ex);
                return false;
            }
            await GetNanoleafDetailedEffectList(nl);
            //hier sollte nun alles vollständig sein und wir können beginnen.
            //State
            if (NLJ.State.Brightness.Value != nl.State.Brightness.Value)
            {
                NLJ.State.Brightness.Value = nl.State.Brightness.Value;
                ManuellStateChange(AuroraConstants.AuroraEvents.Brightness, DateTime.Now);
            }
            if (NLJ.State.ColorMode != nl.State.ColorMode)
            {
                NLJ.State.ColorMode = nl.State.ColorMode;
                ManuellStateChange(AuroraConstants.AuroraEvents.ColorMode, DateTime.Now);
            }
            if (NLJ.State.ColorTemperature.Value != nl.State.ColorTemperature.Value)
            {
                NLJ.State.ColorTemperature.Value = nl.State.ColorTemperature.Value;
                ManuellStateChange(AuroraConstants.AuroraEvents.ColorTemperature, DateTime.Now);
            }
            if (NLJ.State.Hue.Value != nl.State.Hue.Value)
            {
                NLJ.State.Hue.Value = nl.State.Hue.Value;
                ManuellStateChange(AuroraConstants.AuroraEvents.Hue, DateTime.Now);
            }
            if (NLJ.State.Saturation.Value != nl.State.Saturation.Value)
            {
                NLJ.State.Saturation.Value = nl.State.Saturation.Value;
                ManuellStateChange(AuroraConstants.AuroraEvents.Saturation, DateTime.Now);
            }
            if (NLJ.State.Powerstate.Value != nl.State.Powerstate.Value)
            {
                NLJ.State.Powerstate.Value = nl.State.Powerstate.Value;
                ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
            }
            //effects
            if (NLJ.Effects.Selected != nl.Effects.Selected)
            {
                NLJ.Effects.Selected = nl.Effects.Selected;
                ManuellStateChange(AuroraConstants.AuroraEvents.SelectedScenario, DateTime.Now);
            }
            if (JsonConvert.SerializeObject(NLJ.Effects.Scenarios) != JsonConvert.SerializeObject(nl.Effects.Scenarios))
            {
                NLJ.Effects.Scenarios = nl.Effects.Scenarios;
                NLJ.Effects.ScenariosDetailed = nl.Effects.ScenariosDetailed;
                ManuellStateChange(AuroraConstants.AuroraEvents.Scenarios, DateTime.Now);
            }
            return true;
        }
        /// <summary>
        /// Public Setter for ColorTemperature
        /// </summary>
        /// <param name="newvalue"></param>
        /// <returns></returns>
        public async Task<string> SetColorTemperature(int newvalue)
        {
            try
            {
                if (newvalue > NLJ.State.ColorTemperature.Max || newvalue < NLJ.State.ColorTemperature.Min)
                {
                    return "ColorTemperature value out of Range";
                }
                var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, AuroraConstants.Statepath, "{\"ct\":{\"value\":" + newvalue + "}}");
                if (retval == AuroraConstants.RetvalPutPostOK)
                {
                    ColorTemperature = newvalue;
                    ManuellStateChange(AuroraConstants.AuroraEvents.ColorTemperature, DateTime.Now);
                    if (!PowerOn)
                    {
                        PowerOn = true;
                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetColorTemperature", ex);
                return ex.Message;
            }
        }
        public async Task<string> SetPowerOn(Boolean newvalue, Boolean ignoreOldValue = false)
        {
            try
            {
                if (newvalue != NLJ.State.Powerstate.Value || ignoreOldValue)
                {
                    string retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, AuroraConstants.Statepath,
                        "{\"on\":{\"value\":" + newvalue.ToString().ToLower() + "}}");
                    if (retval == AuroraConstants.RetvalPutPostOK)
                    {
                        PowerOn = newvalue;
                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                        return retval;
                    }
                    else
                    {
                        AuroraConstants.log.TraceLog("SetPowerOn", "unerwarteter retval:" + retval);
                    }

                }
                return "nothing Changed on PowerOn";
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetPowerOn", ex);
                return ex.Message;
            }
        }
        public async Task<string> SetSelectedScenario(string newvalue)
        {
            try
            {
                if (NLJ.Effects.Scenarios.Contains(newvalue))
                {

                    string jsontemp = "{\"select\":\"" + newvalue + "\"}";
                    var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, "/effects", jsontemp);
                    if (retval == AuroraConstants.RetvalPutPostOK)
                    {
                        SelectedScenario = newvalue;
                        ManuellStateChange(AuroraConstants.AuroraEvents.SelectedScenario, DateTime.Now);
                        if (!PowerOn)
                        {
                            PowerOn = true;
                            ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                        }
                    }
                    return retval;
                }
                else
                {
                    return "Value not found";
                }
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetSelectedScenario", ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// Set Brightness
        /// </summary>
        /// <param name="newvalue"></param>
        /// <param name="duration">Duration in Seconds</param>
        /// <returns></returns>
        public async Task<string> SetBrightness(int newvalue, int duration = 0)
        {
            try
            {
                if (newvalue > NLJ.State.Brightness.Max || newvalue < NLJ.State.Brightness.Min)
                {
                    return "Brightness value out of Range";
                }
                var json = "{\"brightness\":{\"value\":" + newvalue + "}}";
                if (duration > 0)
                    json = "{\"brightness\":{\"value\":" + newvalue + ",\"duration\":" + duration + "}}";
                var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, AuroraConstants.Statepath, json);
                if (retval == AuroraConstants.RetvalPutPostOK)
                {
                    Brightness = newvalue;
                    ManuellStateChange(AuroraConstants.AuroraEvents.Brightness, DateTime.Now);
                    if (!PowerOn)
                    {
                        PowerOn = true;
                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetBrightness", ex);
                return ex.Message;
            }
        }
        /// <summary>
        /// Set new Hue Value
        /// </summary>
        /// <param name="newvalue"></param>
        /// <returns></returns>
        public async Task<string> SetHue(int newvalue)
        {
            try
            {
                if (newvalue > NLJ.State.Hue.Max || newvalue < NLJ.State.Hue.Min)
                {
                    return "Hue value out of Range";
                }
                var retval = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, AuroraConstants.Statepath, "{\"hue\":{\"value\":" + newvalue + "}}");
                if (retval == AuroraConstants.RetvalPutPostOK)
                {
                    Hue = newvalue;
                    ManuellStateChange(AuroraConstants.AuroraEvents.Hue, DateTime.Now);
                    if (!PowerOn)
                    {
                        PowerOn = true;
                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                    }
                }
                return retval;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("SetHue", ex);
                return ex.Message;
            }
        }
        #endregion PublicMethods
        #region PublicProperties
        public DateTime LastChange { get; private set; } = DateTime.Now;
        /// <summary>
        /// GET/SET the Powerstate
        /// </summary>
        public Boolean PowerOn
        {
            get => NLJ.State.Powerstate.Value;
            private set
            {
                try
                {
                    if (value != NLJ.State.Powerstate.Value)
                    {
                        NLJ.State.Powerstate.Value = value;
                    }
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("PowerOn", ex);
                }
            }
        }
        /// <summary>
        /// Selected Scenario
        /// </summary>
        public String SelectedScenario
        {
            get => NLJ.Effects.Selected;
            private set
            {
                try
                {
                    if (NLJ.Effects.Scenarios.Contains(value))
                    {
                        NLJ.Effects.Selected = value;
                    }
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("SelectedScenario", ex);
                }
            }
        }
        /// <summary>
        /// All Knowing Scenarios
        /// </summary>
        public List<String> Scenarios => NLJ.Effects.Scenarios;
        /// <summary>
        /// ColorMode
        /// </summary>
        public string ColorMode
        {
            get => NLJ.State.ColorMode;
            private set
            {
                try
                {
                    NLJ.State.ColorMode = value;
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("Brightness", ex);
                }
            }
        }
        /// <summary>
        /// Helligkeit
        /// </summary>
        public int Brightness
        {
            get => NLJ.State.Brightness.Value;
            private set
            {
                try
                {
                    if (value > NLJ.State.Brightness.Max || value < NLJ.State.Brightness.Min)
                    {
                        return;
                    }
                    NLJ.State.Brightness.Value = value;
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("Brightness", ex);
                }
            }
        }
        /// <summary>
        /// Farbton (Hue)
        /// </summary>
        public int Hue
        {
            get => NLJ.State.Hue.Value;
            private set
            {
                try
                {
                    NLJ.State.Hue.Value = value;
                    NLJ.Effects.Selected = AuroraConstants.Solid;
                    NLJ.State.ColorMode = AuroraConstants.ColorMode;
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("Hue", ex);
                }
            }
        }
        /// <summary>
        /// Saturation
        /// </summary>
        public int Saturation
        {
            get => NLJ.State.Saturation.Value;
            private set
            {
                try
                {
                    if (value > NLJ.State.Saturation.Max || value < NLJ.State.Saturation.Min)
                    {
                        return;
                    }
                    NLJ.State.Saturation.Value = value;
                    NLJ.Effects.Selected = AuroraConstants.Solid;
                    NLJ.State.ColorMode = AuroraConstants.ColorMode;
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("Saturation", ex);
                }
            }
        }
        /// <summary>
        /// ColorTemperature
        /// </summary>
        public int ColorTemperature
        {
            get => NLJ.State.ColorTemperature.Value;
            private set
            {
                try
                {
                    if (value > NLJ.State.ColorTemperature.Max || value < NLJ.State.ColorTemperature.Min)
                    {
                        return;
                    }
                    NLJ.State.ColorTemperature.Value = value;
                }
                catch (Exception ex)
                {
                    AuroraConstants.log.ServerErrorsAdd("ColorTemperature", ex);
                }
            }
        }
        /// <summary>
        /// This Object is generated by the Json Informations of the Nanoleaf
        /// </summary>
        [DataMember]
        public NanoLeafJson NLJ { get; private set; }
        /// <summary>
        /// User Token
        /// </summary>
        [DataMember]
        public String Token { get; private set; }

        [DataMember]
        public Boolean NewAurora { get; private set; } = false;
        /// <summary>
        /// IP of Aurora
        /// </summary>
        [DataMember]
        public String Ip { get; private set; }
        /// <summary>
        /// SerialNumber of Aurora
        /// </summary>
        [DataMember]
        public String SerialNo { get; private set; }
        [DataMember]
        public String Name { get; set; }
        [DataMember]
        public String OpenAPISupportetFirmwareVersion => "3.2.0";
        #endregion PublicProperties
        #region PrivateMethods
        /// <summary>
        /// Fired Events from The Device
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _auroraEvent_Aurora_Subscriped_Event_Fired(object sender, AuroraFiredEvent e)
        {
            AuroraConstants.log.InfoLog("Aurora DEvice:" + Name, " hat ein Event gefeuert.ID:" + e.ID.ToString());
            foreach (AuroraFiredEventValue item in e.events)
            {
                //Schleife durchlaufen und je nach Event drauf reagieren.
                switch (e.ID)
                {
                    case EventIDTypes.State:
                        Enum.TryParse<EventIDStateAttributtes>(item.attr, out EventIDStateAttributtes eISA);
                        switch (eISA)
                        {
                            case EventIDStateAttributtes.on:
                                if (Boolean.TryParse(item.value, out Boolean newpower))
                                {
                                    if (PowerOn != newpower)
                                    {
                                        PowerOn = newpower;
                                        ManuellStateChange(AuroraConstants.AuroraEvents.Power, DateTime.Now);
                                    }
                                }

                                break;
                        }
                        break;
                    case EventIDTypes.Effects:

                        break;
                }
            }
        }
        /// <summary>
        /// Connect to the Naoleaf
        /// </summary>
        /// <param name="nr">RequestType</param>
        /// <param name="call">Call need to get State of Something like PowerOn (Path)</param>
        /// <param name="value">Value to set on PUT or Post</param>
        /// <returns>Return OK for no Content Pages or the Content</returns>
        private async Task<String> ConnectToNanoleaf(AuroraConstants.RequestTypes nr, string call, string value = "", Boolean retry = false)
        {

            Uri urlstate = new Uri("http://" + Ip + ":" + AuroraConstants.Port + AuroraConstants.Apipath + call);
            //AuroraConstants.log.TraceLog("ConnectToNanoleaf", urlstate.ToString()+" value:"+value);
            HttpResponseMessage result;
            string returnValue;
            try
            {
                //URl aufbauen
                if (!string.IsNullOrEmpty(call))
                {
                    if (call == "new")
                    {
                        urlstate = new Uri("http://" + Ip + ":" + AuroraConstants.Port + AuroraConstants.Apipath + call);
                    }
                    else
                    {
                        if (call == "INIT")
                            call = String.Empty;
                        urlstate = new Uri("http://" + Ip + ":" + AuroraConstants.Port + AuroraConstants.Apipath + Token + call);
                    }
                }
                if (nr == AuroraConstants.RequestTypes.GET)
                {
                    result = await _httpClient.GetAsync(urlstate);
                    returnValue = await result.Content.ReadAsStringAsync();
                }
                else
                {
                    using (var content = new StringContent(value, System.Text.Encoding.UTF8, "application/json"))
                    {
                        if (nr == AuroraConstants.RequestTypes.POST)
                        {
                            result = await _httpClient.PostAsync(urlstate, content);
                        }
                        else
                        {
                            result = await _httpClient.PutAsync(urlstate, content);
                        }
                        if (result.StatusCode == System.Net.HttpStatusCode.NoContent)
                        {
                            returnValue = AuroraConstants.RetvalPutPostOK;
                        }
                        else
                        {
                            returnValue = result.Content.ReadAsStringAsync().Result;
                        }
                    }
                }
                return returnValue;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("ConnectToNanoleaf", ex, "Call:" + urlstate + " Value:" + value + " Retry:" + retry);
                if (!retry)
                {
                    return await ConnectToNanoleaf(nr, call, value, true);
                }
                else
                {
                    return String.Empty;
                }
            }
        }
        private async Task<Boolean> GetNanoleafDetailedEffectList(NanoLeafJson n = null)
        {
            if (n == null)
            {
                n = NLJ;
            }
            var json = await ConnectToNanoleaf(AuroraConstants.RequestTypes.PUT, "/effects", "{\"write\":{\"command\":\"requestAll\"}}");
            var yy = JsonConvert.DeserializeObject(json);
            if (String.IsNullOrEmpty(json))
            {
                return false;
            }
            try
            {
                using (var ms = new MemoryStream(Encoding.Unicode.GetBytes(json)))
                {
                    // Deserialization from JSON  
                    DataContractJsonSerializer deserializer = new DataContractJsonSerializer(typeof(NanoLeafJsonDetailedEffectListAnimationRoot));
                    var xx = (NanoLeafJsonDetailedEffectListAnimationRoot)deserializer.ReadObject(ms);
                    if (n != null)
                        n.Effects.ScenariosDetailed = xx;
                }
                return true;
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("GetNanoleafDetailedEffectList", ex);
                return false;
            }
        }
        /// <summary>
        /// Dient dazu manuelle Änderungen als Event zu feuern und den LastChange entsprechend zu setzen.
        /// </summary>
        /// <param name="_lastchange"></param>
        internal void ManuellStateChange(AuroraConstants.AuroraEvents t, DateTime _lastchange)
        {
            try
            {
                if (Aurora_Changed == null) return;
                LastChange = _lastchange;
                Aurora_Changed(t, this);
            }
            catch (Exception ex)
            {
                AuroraConstants.log.ServerErrorsAdd("ManuellStateChange", ex);
            }
        }
        #endregion PrivateMethods
    }

}