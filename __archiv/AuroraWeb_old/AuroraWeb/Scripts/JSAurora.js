"use strict";
function NanoleafAurora(option) {
    var BasePath = '/api/aurora/';
    var internalaurora = this;
    var _data;
    var _Wrapper = $("#" + option.Wrapper);
    var _PowerDom = [];
    var _SelectedScenarioClass = 'ssc';
    var _BrightnessDOM = [];
    var _BrightnessSliderValue = [];
    var _HueDOM = [];
    this.d2 = [];
    var _HueSliderValue = [];
    var _SaturationDOM = [];
    var _SaturationSliderValue = [];
    var _opjectname = option.Name || "aurora";
    var Timer = 0;
    var GroupScenariousRendered = false;
    var _newData = true;
    var SSE_Event_Source;
    var LastEventID;
    var CallServer = function (url) {
        return $.ajax({
            type: "GET",
            url: BasePath + url,
            dataType: "json"
        });
    };
    this.Eventing = function(){
        if (typeof window.EventSource === "undefined") {
            //ie also return
            return;
        }

        SSE_Event_Source = new window.EventSource("/api/Event/Get");
        SSE_Event_Source.onopen = function (event) {
            //console.log("Event:Connection Opened " + event.data);
        };
        SSE_Event_Source.onerror = function (event) {
            if (event.eventPhase === window.EventSource.CLOSED) {
                console.log("Event:Connection Closed " + event);
                aurora.Eventing();

            } else {
                console.log("Event:Connection Closed Spezial " + event.data);
            }
        };
        SSE_Event_Source.onmessage = function (event) {
            try {
                if (typeof event.data === "undefined" || event.data === "") {
                    return;
                }
                aurora.CheckAuroraEventData(event);
            } catch (ex) {
                console.log("Fehlerhafte Event Daten:" + event.data);
                console.log(ex);
            }
        };
        console.log("SSE started");
    };
    this.CheckAuroraEventData = function(event) {
        try {
            var data = JSON.parse(event.data);
            LastEventID = parseInt(data.ChangedValues.EventID);
            var dataLastChange = new Date(data.LastChange);
            console.log(data);
            var aur = this.GetAurora(data.Serial);
            if (!aur) {
                console.log("aurora konnte für dieses Event nicht ermittelt werden.")
                return false;
            }
            var change = false;
            switch (data.ChangeType) {
                case "Power":
                    var p = data.ChangedValues.Power === "true";
                    if (aur.state.on.value !== p) {
                        aur.state.on.value = p;
                        change = true;
                    }
                    break;
                case "SelectedScenario":
                    if (aur.effects.select !== data.ChangedValues.SelectedScenario) {
                        aur.effects.select = data.ChangedValues.SelectedScenario;
                        change = true;
                    }
                    break;
                case "Brightness":
                    if (aur.state.brightness.Value !== data.ChangedValues.Brightness) {
                        aur.state.brightness.Value = data.ChangedValues.Brightness;
                        change = true;
                    }
                    break;
                case "ColorMode":
                    if (aur.state.colorMode !== data.ChangedValues.ColorMode) {
                        aur.state.colorMode = data.ChangedValues.ColorMode;
                        change = true;
                    }
                    break;
                case "ColorTemperature":
                    if (aur.state.ct.Value !== data.ChangedValues.ColorTemperature) {
                        aur.state.ct.Value = data.ChangedValues.ColorTemperature;
                        change = true;
                    }
                    break;
                case "Hue":
                    if (aur.state.hue.Value !== data.ChangedValues.Hue) {
                        aur.state.hue.Value = data.ChangedValues.Hue;
                        change = true;
                    }
                    break;
                case "Saturation":
                    if (aur.state.sat.Value !== data.ChangedValues.Saturation) {
                        aur.state.sat.Value = data.ChangedValues.Saturation;
                        change = true;
                    }
                    break;
                case "NewNLJ":
                    //hier nur null Prüfung und dann neu Rendern
                    if (aur === null) {
                        aur = data.ChangedValues.NewNLJ;
                    }
                    change = true;
                    break;
                case "Scenarios":
                    if (JSON.stringify(aurora.NLJ.effects.effectsList) !== JSON.stringify(data.ChangedValues.Scenarios)) {
                        aur.effects.effectsList = data.ChangedValues.Scenarios;
                        aur.effects.effectsListDetailed = data.ChangedValues.ScenariosDetailed
                        change = true;
                    }
                    break;
                default:
                    console.log("Unbekannt:" + data.ChangeType);
                    break;
            }
            if (change === true) {
                this.RenderAurora();
            }
        }
        catch (ex) {
            console.log(ex);
            console.log(event);
        }
    }
    this.SetGroupScenario = function (v) {
        var sgs = CallServer("SetGroupScenario/" + v);
        sgs.success(function (data) {
            if (data !== "Done") {
                alert("Es ist ein Fehler aufgetreten:" + data);
            }
            internalaurora.UpdateData();
        });

    };
    this.GetGroupScenarios = function() {
        if (GroupScenariousRendered === true) return true;
        var ggs = CallServer("GetGroupScenario/0");
        ggs.success(function (data) {
            if (data === null) {
                window.setTimeout("aurora.GetGroupScenarios()", 25000);
                return;
            }
            var newdiv = $('<div class="groupcontainer" id="GroupPowerScenarios"></div>');
            newdiv.appendTo($("#Aurora_Group"));
            var gs = $("#GroupPowerScenarios");
            for (var i = 0; i < data.length; i++) {
                var newscendiv = $('<div class="groupscenario">' + data[i] + '</div>');
                newscendiv.appendTo(gs);
                newscendiv.on("click", function () {
                    internalaurora.SetGroupScenario($(this).html());
                });
            }
            GroupScenariousRendered = true;
        });
    }
    this.GetAurora = function (serial) {
        for (var i = 0; i < _data.length; i++) {
            if (_data[i].NewAurora === true || _data[i].NLJ ===null) continue;
            var s = _data[i].NLJ.serialNo;
            if (s === serial) {
                return _data[i].NLJ;
            }
        }
        return false;
    };
    this.SetPowerState = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (typeof v === "boolean" && v !== au.state.on.value) {
            CallServer("SetPowerState/" + serial + "/" + v);
            au.state.on.value = v;
            this.RenderAurora();
        }
    };
    this.SetHue = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (v !== au.state.hue.value && v >= au.sate.hue.min && v <= au.sate.hue.max) {
            CallServer("SetHue/" + serial + "/" + v);
            au.state.hue.value = v;
            this.RenderAurora();
        }
    };
    this.SetSaturation = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (v !== au.state.sat.value && v >= au.sate.sat.min && v <= au.sate.sat.max) {
            CallServer("SetHue/" + serial + "/" + v);
            au.state.hue.value = v;
            this.RenderAurora();
        }
    };
    this.SetPower = function (serial) {
        var t = _PowerDom[serial].prop("checked");
        internalaurora.SetPowerState(!t, serial);
    };
    this.SetSelectedScenario = function (v, serialNo) {
        var au = this.GetAurora(serialNo);
        if (au === false) return;
        au.effects.select = v;
        if (au.state.on.value !== true) {
            au.state.on.value = true;
        }
        CallServer("SetSelectedScenario/" + serialNo + "/" + v);
        this.RenderAurora();
        return;
    };
    this.RenderAurora = function () {
        if (typeof _data === "undefined") {
            alert("Aurora ist nicht initialisiert");
            return false;
        }
        if (_newData === true) {
            for (var x = 0; x < _data.length; x++) {
                var faid = "new";
                if (_data[x].NewAurora === false) {
                    faid = _data[x].SerialNo;
                }
                var newAurora = $('<div id="Aurora_' + faid + '" class="auroraContainer"><div class="auroraName">' + _data[x].Name + '</div><div class="container"><input type="checkbox" onClick="' + _opjectname + '.SetPower(\'' + faid + '\')" id="power_' + faid + '" class="powerCheck" name="power_' + faid + '" checked="checked"/><label for="power_' + faid + '" class="power"><span class="icon-off"></span><span class="light"></span></label></div><div class="brightnessSlider" id="BrightnessSlider_' + faid + '" data="' + faid + '"><div id="BrightnessSliderLabel_' + faid + '" class="brightnessSliderLabel">Helligkeit</div><div id="BrightnessSliderValue_' + faid + '" class="brightnessSliderValue">50</div></div><div class="hueSlider" id="HueSlider_' + faid + '" data="' + faid + '"><div id="HueSliderLabel_' + faid + '" class="hueSliderLabel">Hue</div><div id="HueSliderValue_' + faid + '" class="hueSliderValue">0</div></div><div class="saturationSlider" id="SaturationSlider_' + faid + '" data="' + faid + '"><div id="SaturationSliderLabel_' + faid + '" class="saturationSliderLabel">Saturation</div><div id="SaturationSliderValue_' + faid + '" class="saturationSliderValue">0</div></div><div id="Scenarios_' + faid + '" data="' + faid + '" class="scenarios"></div></div>');
                newAurora.appendTo(_Wrapper);
            }
            if (_data.length > 1) {
                var newAuroraG = $('<div id="Aurora_Group" class="auroraContainer"><div class="auroraName">Alle Aurora</div><div class="groupcontainer" id="GroupPowerOn">Power On</div><div class="groupcontainer" id="GroupPowerOff">Power Off</div></div>');
                newAuroraG.appendTo(_Wrapper);
                $("#GroupPowerOn").on("click", function () {
                    CallServer("SetGroupPowerState/true").success(function () {
                        internalaurora.UpdateData();
                    });

                });
                $("#GroupPowerOff").on("click", function () {
                    CallServer("SetGroupPowerState/false").success(function () {
                        internalaurora.UpdateData();
                    });
                });

            }
            _newData = false;
        }
        if (_data.length > 1) {
            this.GetGroupScenarios();
        }
        for (var i = 0; i < _data.length; i++) {
            //Check new Aurora and continue
            if (_data[i].NewAurora === true || _data[i].NLJ === null) {
                continue;
            }
            var aid = _data[i].SerialNo;
            //Power
            _PowerDom[aid] = $("#power_" + aid);
            if (_PowerDom[aid].prop("checked") !== !_data[i].NLJ.state.on.value) {
                _PowerDom[aid].prop("checked", !_data[i].NLJ.state.on.value);
            }
            //Scenarios
            var effectlist = _data[i].NLJ.effects.effectsListDetailed.animations;
            if (effectlist === 0) {
                alert("Keine Scenarien geliefert Aurora Serial:" + _data[i].SerialNo);
                continue;
            }
            var sd = $("#Scenarios_" + aid);
            sd.empty();
            var internalI = i;
            
            $.each(effectlist, function (index, item) {
                var newdiv;
                var rhythm = "";
                if (item.pluginType === "rhythm") {
                    rhythm = '<img src="/Images/rhythm.jpg" class="rhythmimage">';
                }
                if (item.animName === _data[internalI].NLJ.effects.select) {
                    newdiv = $('<div data="' + item.animName+'" class="aurorascenario ' + _SelectedScenarioClass + '">' + rhythm + item.animName + '</div>');
                } else {
                    newdiv = $('<div data="' + item.animName +'" class="aurorascenario">' + rhythm + item.animName + '</div>');
                    }
                    newdiv.appendTo(sd);
                    newdiv.on("click", function () {
                        var serial = $(this).parent().attr("data");
                        internalaurora.SetSelectedScenario($(this).attr("data"), serial);
                    });
                });
            
            //Brightness
            var brivalue = _data[i].NLJ.state.brightness.value;
            var brimin = _data[i].NLJ.state.brightness.min;
            var brimax = _data[i].NLJ.state.brightness.max;
            
            if (typeof _BrightnessDOM[aid] === "undefined") {
                _BrightnessSliderValue[aid] = $("#BrightnessSliderValue_" + aid);
                _BrightnessSliderValue[aid].html(brivalue);
                _BrightnessDOM[aid] = $("#BrightnessSlider_" + aid);
                _BrightnessDOM[aid].slider({
                    orientation: "vertical",
                    range: "min",
                    min: brimin,
                    max: brimax,
                    value: brivalue,
                    stop: function (event, ui) {
                        var serial = $(this).attr("data");
                        var au = internalaurora.GetAurora(serial);
                        if (au === false || typeof au.state ==="undefined" || au.state === null) return false;
                        au.state.brightness.value = ui.value;
                        CallServer("SetBrightness/" + serial + "/" + ui.value);
                        if (au.state.on.value !== true) {
                            au.state.on.value = true;
                            internalaurora.RenderAurora();
                        }
                        return true;
                    },
                    slide: function (event, ui) {
                        var serial = $(this).attr("data");
                        _BrightnessSliderValue[serial].html(ui.value);
                    }
                });
            } else {
                if (_BrightnessDOM[aid].slider("option", "value") !== brivalue) {
                    _BrightnessDOM[aid].slider({ value: brivalue });
                    _BrightnessSliderValue[aid].html(brivalue);
                }
            }
            //Hier nun die besonderheiten abarbeiten für hue und sat
            var currenthue = _data[i].NLJ.state.hue.value;
            var huemax = _data[i].NLJ.state.hue.max;
            var huemin = _data[i].NLJ.state.hue.min;
            var currentsat = _data[i].NLJ.state.sat.value;
            var satmax = _data[i].NLJ.state.sat.max;
            var satmin = _data[i].NLJ.state.sat.min;
                if (_data[i].NLJ.effects.select !== "*Solid*") {
                    currenthue = 0;
                    currentsat = 0;
                }
            //hue
            if (typeof _HueDOM[aid] === "undefined") {
                _HueSliderValue[aid] = $("#HueSliderValue_" + aid);
                _HueSliderValue[aid].html(currenthue);
                _HueDOM[aid] = $("#HueSlider_" + aid);
                this.d2[aid] = _HueDOM[aid];
                _HueDOM[aid].slider({
                    orientation: "vertical",
                    range: "min",
                    min: huemin,
                    max: huemax,
                    value: currenthue,
                    stop: function (event, ui) {
                        var serial = $(this).attr("data");
                        var au = internalaurora.GetAurora(serial);
                        if (au === false || typeof au.state === "undefined" || au.state === null) return false;
                        au.state.hue.value = ui.value;
                        CallServer("SetHue/" + serial + "/" + ui.value);
                        if (au.state.on.value !== true) {
                            au.state.on.value = true;
                        }
                        if (au.effects.select !== "*Solid*") {
                            au.effects.select = "*Solid*";
                        }
                        internalaurora.RenderAurora();
                        return true;
                    },
                    slide: function (event, ui) {
                        var serial = $(this).attr("data");
                        _HueSliderValue[serial].html(ui.value);
                    }
                });

            } else {
                if (_HueDOM[aid].slider("option", "value") !== currenthue) {
                    _HueDOM[aid].slider({ value: currenthue });
                    _HueSliderValue[aid].html(currenthue);
                }
            }
            //Saturation
            if (typeof _SaturationDOM[aid] === "undefined") {
                _SaturationSliderValue[aid] = $("#SaturationSliderValue_" + aid);
                _SaturationSliderValue[aid].html(currentsat);
                _SaturationDOM[aid] = $("#SaturationSlider_" + aid).slider({
                    orientation: "vertical",
                    range: "min",
                    min: satmin,
                    max: satmax,
                    value: currentsat,
                    stop: function (event, ui) {
                        var serial = $(this).attr("data");
                        var au = internalaurora.GetAurora(serial);
                        if (au === false || typeof au.state === "undefined" || au.state === null) return false;
                        au.state.sat.value = ui.value;
                        CallServer("SetSaturation/" + serial + "/" + ui.value);
                        if (au.state.on.value !== true) {
                            au.state.on.value = true;
                        }
                        if (au.effects.select !== "*Solid*") {
                            au.effects.select = "*Solid*";
                        }
                        internalaurora.RenderAurora();
                        return true;
                    },
                    slide: function (event, ui) {
                        var serial = $(this).attr("data");
                        _SaturationSliderValue[serial].html(ui.value);
                    }
                });
            } else {
                if (_SaturationDOM[aid].slider("option", "value") !== currentsat) {
                    _SaturationDOM[aid].slider({ value: currentsat });
                    _SaturationSliderValue[aid].html(currentsat);
                }
            }
        }

        return true;

    };
    this.Init = function () {
        this.UpdateData();
        this.Eventing();
    };
    this.UpdateData = function () {
        clearTimeout(Timer);
        //Init Nanaoleaf && Get Server Data
        var request = CallServer("Get");
        request.success(function (data) {
            var pl = $("#PreLoad");
            if (pl.is(":visible")) {
                pl.hide();
            }
            //if (data.length === 0 || data[0].NewAurora === false && (data[0].NLJ === null || typeof data[0].NLJ === "undefined" || typeof data[0].NLJ.name === "undefined")) {
            //    if (!_Wrapper.is(":empty")) {
            //        _Wrapper.empty();
            //    }
            //    console.log("Fehler beim Initialisieren: Object enthält kein Namen");
            //} else {

                if (typeof _data === "undefined" || _data.length !== data.length) {
                    _newData = true;
                    _Wrapper.empty();
                    _PowerDom = [];
                    _BrightnessDOM = [];
                    _HueDOM = [];
                    _SaturationDOM = [];
                } else {
                    _newData = false;
                }
                _data = data;
                internalaurora.d = data;
                internalaurora.RenderAurora();
            //}
            return true;
        }).fail(function () {
            alert("Initialisierung fehlgeschlagen.");
        });
        //window.setTimeout(_opjectname + ".UpdateData()", 30000);
    };


}