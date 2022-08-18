using HomeLogging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Zeroconf;
namespace Aurora
{
    public static class AuroraWrapper
    {
        #region ClassVariables
        public static event EventHandler<AuroraLigth> Auroras_Changed = delegate { };
        private static readonly Logging log = new Logging(new LoggerWrapperConfig { ErrorFileName = "AuroraErrors.txt", TraceFileName = "AuroraTrace.txt", InfoFileName = "AuroraInfo.txt", ConfigName = "Aurora", AddDateTimeToFilesNames = true });
        private static List<AuroraKnowingDevices> _knowingAuroras = new List<AuroraKnowingDevices>();
        private static List<String> _groupScenarios;
        private static String Configpath = String.Empty;
        #endregion ClassVariables
        #region Private Methods
        /// <summary>
        /// Liest die Config Datei der Bekannten Auroren
        /// </summary>
        /// <returns></returns>
        private static List<AuroraKnowingDevices> ReadAuroraKnowingDevicesXml()
        {
            List<AuroraKnowingDevices> akd = new List<AuroraKnowingDevices>();
            try
            {

                string path = Configpath + "Configuration\\AuroraKnowingDevices.xml";
                if (string.IsNullOrEmpty(Configpath) || !File.Exists(path)) return akd;
                XmlDocument myXmlDocument = new XmlDocument();
                myXmlDocument.Load(path);
                XmlNodeList Auroraconfig = myXmlDocument.SelectNodes("/AuroraKnowingDevices/AuroraKnowingDevice");
                foreach (XmlNode item in Auroraconfig)
                {
                    try
                    {
                        Boolean.TryParse(item.Attributes["UseSubscription"]?.Value, out Boolean useSubscription);
                        AuroraKnowingDevices st = new AuroraKnowingDevices(
                            item.Attributes["MacAdress"].Value,
                            item.Attributes["AuthToken"].Value,
                            item.Attributes["DeviceName"].Value,
                            item.Attributes["KnowingIP"].Value,
                            item.Attributes["Serial"].Value, useSubscription);
                        if (!akd.Any())
                        {
                            akd.Add(st);
                        }
                        else
                        {
                            AuroraKnowingDevices curakd = akd.FirstOrDefault(x => x.MacAdress == st.MacAdress);
                            if (curakd == null)
                            {
                                akd.Add(st);
                            }
                            else
                            {
                                curakd.MacAdress = st.MacAdress;
                                curakd.AuthToken = st.AuthToken;
                                curakd.DeviceName = st.DeviceName;
                                curakd.KnowingIP = st.KnowingIP;
                                curakd.Serial = st.Serial;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }


                }
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("AuroraWrapper:ReadAuroraKnowingDevicesXml" + ex.Message, null);
            }
            return akd;
        }
        /// <summary>
        /// Generiert die Liste mit Bekannten Geräten aus Config oder Code
        /// </summary>
        /// <returns></returns>
        private static List<AuroraKnowingDevices> GenerateKnowingDevices()
        {
            if (_knowingAuroras.Any()) return _knowingAuroras;
            _knowingAuroras = ReadAuroraKnowingDevicesXml();
            if (_knowingAuroras.Any()) return _knowingAuroras;


            _knowingAuroras = new List<AuroraKnowingDevices>()
            {
                new AuroraKnowingDevices("C8:EF:29:5C:91:24", "p7rY1vD2YxRQLkLZ8SxhYtVsIhCTMsp3", "Wohnzimmer","192.168.0.110","S16432A0525"),
                new AuroraKnowingDevices("94:9F:5B:E9:5F:A8", "vD2YxRQLkLZ8SxhYtVsIhCTMsp3ws5A4", "Esszimmer","192.168.0.112","S17122A4899")

            };
            return _knowingAuroras;

        }
        /// <summary>
        /// Discover Nanoleafs in Local Network
        /// </summary>
        /// <returns>List of Founded auroras in Network</returns>
        private static async Task<List<AuroraSearchResults>> FindAuroras()
        {
            try
            {
                List<AuroraSearchResults> lasr = new List<AuroraSearchResults>();
                IReadOnlyList<IZeroconfHost> results = await ZeroconfResolver.ResolveAsync("_nanoleafapi._tcp.local.", new TimeSpan(0, 0, 0, 5)).ConfigureAwait(true);

                if (results.Count == 0) return lasr;
                foreach (IZeroconfHost host in results)
                {
                    AuroraSearchResults asr = new AuroraSearchResults(host.IPAddress,
                        host.Services.First().Value.Properties.First().First().Value, host.Services.First().Value.Port);
                    lasr.Add(asr);
                }
                return lasr;
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("error in private Method: " + ex.Message, null);
                return null;
            }
        }
        /// <summary>
        /// Start Search for Auroras in NEtwork
        /// Build List of Auroras include New and knowed Devices
        /// </summary>
        /// <returns>List of All Auroras</returns>
        private static async Task<List<AuroraLigth>> Discovery(Boolean withDiscovery = false)
        {
            try
            {
                if (AurorasList == null || AurorasList.Count == 0)
                    AurorasList = new List<AuroraLigth>();
                //Start to Search

                List<AuroraSearchResults> lasr = new List<AuroraSearchResults>();

                if (withDiscovery)
                {
                    lasr = await FindAuroras();
                }
                if (lasr.Count > 0)
                {

                    foreach (AuroraSearchResults asrResults in lasr)
                    {
                        AuroraKnowingDevices akd = _knowingAuroras.FirstOrDefault(x => x.MacAdress == asrResults.MACAdress);
                        if (akd != null)
                        {
                            AuroraLigth a = new AuroraLigth(akd.AuthToken, asrResults.IP, akd.DeviceName, akd.Serial, akd.UseSubscription);
                            var t = AurorasList.FirstOrDefault(x => x.Ip == a.Ip);
                            if (t != null)
                            {
                                if (t.NLJ == null)
                                    await t.GetNanoLeafInformations();
                            }
                            else
                            {
                                if (a.NLJ == null)
                                    await a.GetNanoLeafInformations();
                                a.Aurora_Changed += AuroraChanged;
                                AurorasList.Add(a);
                            }
                        }
                        else
                        {
                            AuroraLigth a = new AuroraLigth("new", asrResults.IP, "New");
                            var t = AurorasList.FirstOrDefault(x => x.Ip == asrResults.IP);
                            if (t == null)
                            {
                                a.Aurora_Changed += AuroraChanged;
                                AurorasList.Add(a);
                            }
                        }
                    }
                }
                //Check for Knowing Devices
                if (_knowingAuroras.Count > 0)
                {
                    foreach (AuroraKnowingDevices auroraKnowingDevice in _knowingAuroras)
                    {
                        if (!string.IsNullOrEmpty(auroraKnowingDevice.KnowingIP))
                        {
                            var t = AurorasList.FirstOrDefault(x => x.Ip == auroraKnowingDevice.KnowingIP);
                            if (t == null)
                            {
                                //FindAurora havent Found this Aurora so add this to list
                                AuroraLigth a = new AuroraLigth(auroraKnowingDevice.AuthToken, auroraKnowingDevice.KnowingIP, auroraKnowingDevice.DeviceName, auroraKnowingDevice.Serial, auroraKnowingDevice.UseSubscription);
                                bool re = true;
                                if (a.NLJ == null)
                                {
                                    re = await a.GetNanoLeafInformations();
                                }
                                //If false there is an error on init.
                                a.Aurora_Changed += AuroraChanged;
                                AurorasList.Add(a);
                            }

                        }
                    }

                }
                return AurorasList;
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("Fehler in privater Methode:" + ex.Message, null);
                return null;
            }

        }

        private static void AuroraChanged(object sender, AuroraLigth e)
        {
            Auroras_Changed(sender, e);
        }

        /// <summary>
        /// Chekt the AuroraLiving. If false return.
        /// </summary>
        /// <returns></returns>
        public async static Task<Boolean> CheckAuroraLiving()
        {
            try
            {
                if (AurorasList.Count == 0)
                {
                    await InitAuroraWrapper();
                }
                foreach (AuroraLigth aurora in AurorasList)
                {
                    if (aurora.NLJ == null)
                    {
                        await aurora.GetNanoLeafInformations();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("CheckAuroraLiving", ex);
                return false;
            }
        }
        #endregion Private Methods
        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="KnowingAuroras"></param>
        /// <param name="withDiscovery">DEfault false, False mean there is no searching Auroras, You need KnowingAuroras</param>
        /// <returns></returns>
        public static async Task<List<AuroraLigth>> InitAuroraWrapper(string _Configpath = null, Boolean withDiscovery = false)
        {
            if (string.IsNullOrEmpty(_Configpath))
            {
                _Configpath = System.AppDomain.CurrentDomain.BaseDirectory;
            }
            Configpath = _Configpath;
            _knowingAuroras = GenerateKnowingDevices();
            await Discovery(withDiscovery);
            return AurorasList;
        }
        /// <summary>
        /// Get Aurora Object by Serial
        /// </summary>
        /// <param name="serial">Serial String of a Knowing Aurora</param>
        /// <returns>Aurora</returns>
        public async static Task<AuroraLigth> GetAurorabySerial(string serial)
        {
            if (!await CheckAuroraLiving()) return null;
            try
            {
                return AurorasList.FirstOrDefault(x => x.SerialNo == serial);
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("GetAurorabySerial Method: " + ex.Message, null);
                return null;
            }
        }
        /// <summary>
        /// Get Aurora Object by Serial
        /// </summary>
        /// <param name="Name">Name String of a Knowing Aurora</param>
        /// <returns>Aurora</returns>
        public async static Task<AuroraLigth> GetAurorabyName(string name)
        {
            if (!await CheckAuroraLiving()) return null;
            try
            {
                return AurorasList.FirstOrDefault(x => x.Name.ToLower() == name.ToLower());
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("GetAurorabyName Method: " + ex.Message, null);
                return null;
            }
        }
        /// <summary>
        /// Change Powerstate for all Auroras
        /// </summary>
        /// <param name="_poweron"></param>
        /// <param name="ignoreOldValue"></param>
        /// <returns></returns>
        public async static Task<Boolean> GroupPowerOn(Boolean _poweron, Boolean ignoreOldValue = false)
        {
            Boolean retval = true;
            if (!await CheckAuroraLiving()) return false;
            try
            {
                foreach (AuroraLigth aurora in AurorasList)
                {
                    try
                    {
                        if (!aurora.NewAurora && aurora.PowerOn != _poweron || ignoreOldValue)
                        {
                            if (_poweron)
                            {
                                await aurora.SetRandomScenario(true);
                                await aurora.SetBrightness(50, 20);
                            }
                            else
                            {
                                await aurora.SetPowerOn(_poweron, ignoreOldValue);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        retval = false;
                        log.ServerErrorsAdd("GroupPowerOn", ex, aurora.Name);
                    }
                }
                return retval;
            }
            catch
            {
                return false;
            }
        }
        /// <summary>
        /// Merge double Scenarios of all Auroras
        /// </summary>
        /// <returns></returns>
        public async static Task<List<String>> GetGroupScenarios()
        {
            if (_groupScenarios == null || _groupScenarios.Count == 0)
            {
                _groupScenarios = new List<string>();
                List<String> tempgs = new List<string>();
                if (!await CheckAuroraLiving()) return null;

                foreach (AuroraLigth aurora in AurorasList)
                {
                    tempgs = tempgs.Count == 0 ? aurora.Scenarios : tempgs.Intersect(aurora.Scenarios).ToList();
                }
                if (tempgs.Count > 0) _groupScenarios = tempgs;
            }
            return _groupScenarios;

        }
        /// <summary>
        /// Set Group Scenarios
        /// </summary>
        /// <param name="scenario"></param>
        /// <returns></returns>
        public async static Task<String> SetGroupScenarios(string scenario)
        {
            if (!await CheckAuroraLiving()) return null;
            try
            {
                foreach (AuroraLigth aurora in AurorasList)
                {
                    if (aurora.Scenarios.Contains(scenario))
                    {
                        await aurora.SetSelectedScenario(scenario);
                    }
                }
                return "Done";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public async static Task<Boolean> RefreshAruroaProperties()
        {
            try
            {
                if (AurorasList.Count == 0)
                {
                    await InitAuroraWrapper();
                    foreach (AuroraLigth aurora in AurorasList)
                    {
                        if (aurora.NLJ == null)
                        {
                            await aurora.GetNanoLeafInformations();
                            aurora.ManuellStateChange(AuroraConstants.AuroraEvents.NewNLJ, DateTime.Now);
                        }
                    }
                    return true;
                }
                //Hier nun die Properties laden.
                foreach (AuroraLigth aurora in AurorasList)
                {
                    if (aurora.NLJ == null)
                    {
                        await aurora.GetNanoLeafInformations();
                        aurora.ManuellStateChange(AuroraConstants.AuroraEvents.NewNLJ, DateTime.Now);
                        continue;
                    }
                    //hier ist schon ein richtig initialisiertes Objekt.
                    await aurora.RefreshProperties();
                }
                return true;
            }
            catch (Exception ex)
            {
                log.ServerErrorsAdd("RefreshAruroaProperties", ex);
                return false;
            }
        }
        #endregion Public Methods

        #region Propertys
        /// <summary>
        /// List of Knowing / Discovered Auroras
        /// </summary>
        public static List<AuroraLigth> AurorasList { get; private set; } = new List<AuroraLigth>();
        #endregion Propertys
    }




}
