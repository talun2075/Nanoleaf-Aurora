using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using System.Web.Http;
using Aurora;

namespace AuroraWeb.Controllers
{
    /// <summary>
    /// Schnittstelle/API für die Nanoleaf Aurora
    /// </summary>
    public class AuroraController : ApiController
    {
        /// <summary>
        /// Get Data
        /// </summary>
        /// <returns>Nanoleaf Object</returns>
        private static Boolean EventingInited = false;
        [HttpGet]
        public async Task<List<AuroraLigth>> Get()
        {
            await AuroraWrapper.CheckAuroraLiving();
            if (!EventingInited)
            {
                AuroraWrapper.Auroras_Changed += AuroraWrapper_Auroras_Changed;
                EventingInited = true;
            }
            return AuroraWrapper.AurorasList;
        }

        private void AuroraWrapper_Auroras_Changed(object sender, AuroraLigth e)
        {
            EventController.EventAuroraChange((AuroraConstants.AuroraEvents)sender, e);
        }

        [HttpGet]
        public async Task<Boolean> RefreshAuroraProperties()
        {
            return await AuroraWrapper.RefreshAruroaProperties();
        }
        /// <summary>
        /// Set Scenario
        /// </summary>
        /// <param name="id">Name of Scenario</param>
        /// <param name="v"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<string> SetSelectedScenario(string id, string v)
        {
            try
            {
                if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(id)) return null;
                AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
                if (a.Scenarios.Contains(v) && a.SelectedScenario != v)
                {
                    await a.SetSelectedScenario(v);
                }
                return a.SelectedScenario;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        /// <summary>
        /// Set Powerstate
        /// </summary>
        /// <param name="id">Serial</param>
        /// <param name="v">Value of Powerstate</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Boolean> SetPowerState(string id, Boolean v)
        {
            if (string.IsNullOrEmpty(id)) return false;
                AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
                if (a == null) return false;
                if (a.PowerOn != v)
                {
                    await a.SetPowerOn(v);
                }
                return a.PowerOn;
        }
        /// <summary>
        /// Set Powerstate
        /// </summary>
        /// <param name="id">Serial</param>
        /// <param name="v">Value of Powerstate</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Boolean> SetPowerStateIgnoreOldValue(string id, Boolean v)
        {
            if (string.IsNullOrEmpty(id)) return false;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (a == null) return false;
                await a.SetPowerOn(v,true);
            return a.PowerOn;
        }
        /// <summary>
        /// Set Powerstate ignoriert den aktuellen Status
        /// </summary>
        /// <param name="id">NAme</param>
        /// <param name="v">Value of Powerstate</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<Boolean> SetPowerStateByName(string id, string v)
        {
            if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(id)) return false;
            if (Boolean.TryParse(v, out bool po))
            {
                AuroraLigth a = await AuroraWrapper.GetAurorabyName(id);
                if (a == null) return false;
                    await a.SetPowerOn(po);
                return a.PowerOn;
            }
            return false;
        }
        [HttpGet]
        public async Task<Boolean> SetGroupPowerState(Boolean id)
        {
            return await AuroraWrapper.GroupPowerOn(id);
        }
        [HttpGet]
        public async Task<Boolean> SetGroupPowerStateIgnoreOldValue(Boolean id)
        {
            return await AuroraWrapper.GroupPowerOn(id,true);
        }
        /// <summary>
        /// Brightness /Helligkeit
        /// </summary>
        /// <param name="id">Number between min and max</param>
        /// <param name="v">Value of Brightness</param>
        /// <returns>Brightness</returns>
        [HttpGet]
        public async Task<int> SetBrightness(string id, int v)
        {
            if (string.IsNullOrEmpty(id)) return -999;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (v > a.NLJ.State.Brightness.Max || v < a.NLJ.State.Brightness.Min) return 0;
            if (a.NLJ.State.Brightness.Value != v)
            {
                await a.SetBrightness(v, 20);
            }
            return a.Brightness;
        }
        [HttpGet]
        public async Task<int> SetSaturation(string id, int v)
        {
            if (string.IsNullOrEmpty(id)) return -999;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (v > a.NLJ.State.Saturation.Max || v < a.NLJ.State.Saturation.Min) return 0;
            if (a.NLJ.State.Saturation.Value != v)
            {
               await a.SetSaturation(v);
            }
            return a.Saturation;
        }
        [HttpGet]
        public async Task<int> SetColorTemperature(string id, int v)
        {
            if (string.IsNullOrEmpty(id)) return -999;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (v > a.NLJ.State.ColorTemperature.Max || v < a.NLJ.State.ColorTemperature.Min) return 0;
            if (a.NLJ.State.ColorTemperature.Value != v)
            {
                await a.SetColorTemperature(v);
            }
            return a.ColorTemperature;
        }
        /// <summary>
        /// Setzen eins zufälligen Scenarios
        /// </summary>
        /// <param name="serial">Serial of the Aurora</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<String> SetRandomScenario(string id, Boolean v = false)
        {
            if (string.IsNullOrEmpty(id)) return null;
            if(await AuroraWrapper.CheckAuroraLiving())
            {
                AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
                
                return await a.SetRandomScenario(v);
            }
            return "false";
        }
        /// <summary>
        /// Ermitteln der Gruppenscenarien
        /// </summary>
        /// <param name="id">Dummy</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<List<String>> GetGroupScenario(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await AuroraWrapper.GetGroupScenarios();
        }
        /// <summary>
        /// Setzen der Gruppen Scenarien
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<String> SetGroupScenario(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            return await AuroraWrapper.SetGroupScenarios(id);
        }
        [HttpGet]
        public async Task<Boolean> SetHue(string id, int v)
        {
            if (string.IsNullOrEmpty(id)) return false;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (v < a.NLJ.State.Hue.Min || v > a.NLJ.State.Hue.Max) return false;
            await a.SetHue(v);
            return true;
        }
        /// <summary>
        /// Registriert einen neuen User bei allen gefundenen Aurroas.
        /// Funktioniert nur, wenn auch bei der Aurora 5-7 Sekunden geklickt wurde. 
        /// </summary>
        /// <param name="id">IP</param>
        /// <returns>Token</returns>
        [HttpGet]
        public async Task<string> RegisterNewUser(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            AuroraLigth a = new AuroraLigth("New", id, "NewAurora");
            var retval = await a.NewUser();
            if (String.IsNullOrEmpty(retval))
            {
                retval = "Ein Fehler ist aufgetreten";
            }
            return retval;
        }
        /// <summary>
        /// Umbenennen von Scenarien
        /// </summary>
        /// <param name="id">Serial der Auroras</param>
        /// <param name="v">Altes Scenario @ Neues Scenario Beispiel old@new</param>
        /// <returns>True wenn es geklappt hat.</returns>
        [HttpGet]
        public async Task<Boolean> RenameScenario(string id, string v)
        {
            if (string.IsNullOrEmpty(v) || string.IsNullOrEmpty(id)) return false;
            AuroraLigth a = await AuroraWrapper.GetAurorabySerial(id);
            if (a == null || !v.Contains("@")) return false;
            var sp = v.Split('@');
            if (string.IsNullOrEmpty(sp[0]) || string.IsNullOrEmpty(sp[1])) return false;
            return await a.RenameScenario(sp[0], sp[1]);
        }
    }
}
