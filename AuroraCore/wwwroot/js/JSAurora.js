"use strict";
function NanoleafAurora(option) {
    var BasePath = '/aurora/';
    var t = this;
    var _data;
    let auroraWrapper = document.getElementById(option.Wrapper);
    let _PowerDom = [];
    let _SelectedScenarioClass = 'ssc';
    this.ColorPickers = [];
    var _opjectname = option.name || "aurora";
    var Timer = 0;
    var GroupScenariousRendered = false;
    var _newData = true;
    var workedGroupPower = [];
    var SSE_Event_Source;
    this.Eventing = function () {
        if (typeof window.EventSource === "undefined") {
            //ie also return
            return;
        }

        SSE_Event_Source = new EventSource("/Event/");
        SSE_Event_Source.onopen = function () {
            console.log("Event:Connection Opened ");
        };
        SSE_Event_Source.onerror = function (event) {
            console.log("Event:Connection Closed");
            //aurora.Eventing();
        };
        SSE_Event_Source.onmessage = function (event) {
            // Do something with the data:
            console.log(event); // event.data;
        };
        SSE_Event_Source.addEventListener("aurora", function (event) {
            // do stuff with data
            aurora.CheckAuroraEventData(event);
        });
        //SSE_Event_Source.onmessage = function (event) {
        //    console.log("Event:Connection message " + event.data);
        //    try {
        //        if (typeof event.data === "undefined" || event.data === "") {
        //            return;
        //        }
        //        //aurora.CheckAuroraEventData(event);
        //    } catch (ex) {
        //        console.log("Fehlerhafte Event Daten:" + event.data);
        //        console.log(ex);
        //    }
        //};
        console.log("SSE started");
    };
    this.CheckAuroraEventData = function (event) {
        try {
            var data = JSON.parse(event.data);
            console.log(data);
            var aur = this.GetAurora(data.serial);
            if (!aur) {
                console.log("aurora konnte für dieses Event nicht ermittelt werden.")
                return false;
            }
            var change = false;
            switch (data.changeType) {
                case "Power":
                    var p = data.changedValues.Power === "true";
                    if (aur.state.powerstate.value !== p) {
                        aur.state.powerstate.value = p;
                        change = true;
                    }
                    break;
                case "SelectedScenario":
                    if (aur.effects.selected !== data.changedValues.SelectedScenario) {
                        aur.effects.selected = data.changedValues.SelectedScenario;
                        change = true;
                    }
                    break;
                case "Brightness":
                    let br = parseInt(data.changedValues.Brightness);
                    if (aur.state.brightness.value !== br) {
                        aur.state.brightness.value = br;
                        change = true;
                    }
                    break;
                case "ColorMode":
                    if (aur.state.colorMode !== data.changedValues.ColorMode) {
                        aur.state.colorMode = data.changedValues.ColorMode;
                        change = true;
                    }
                    break;
                case "ColorTemperature":
                    let ct = parseInt(data.changedValues.ColorTemperature);
                    if (aur.state.ct.value !== ct) {
                        aur.state.ct.value = ct;
                        change = true;
                    }
                    break;
                case "Hue":
                    let hue = parseInt(data.changedValues.Hue)
                    if (aur.state.hue.value !== hue) {
                        aur.state.hue.value = hue;
                        change = true;
                    }
                    break;
                case "Saturation":
                    let sat = parseInt(data.changedValues.Saturation);
                    if (aur.state.saturation.value !== sat) {
                        aur.state.saturation.value = sat;
                        change = true;
                    }
                    break;
                case "NewNLJ":
                    //hier nur null Prüfung und dann neu Rendern
                    if (aur === null) {
                        aur = data.changedValues.NewNLJ;
                    }
                    change = true;
                    break;
                case "Scenarios":
                    if (JSON.stringify(aurora.nlj.effects.effectsList) !== JSON.stringify(data.changedValues.Scenarios)) {
                        aur.effects.effectsList = data.changedValues.Scenarios;
                        aur.effects.scenariosDetailed = data.changedValues.ScenariosDetailed
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
    this.SetGroupScenario = function (v, room) {
        console.log(room);

        var sgs = this.Send("SetGroupScenario/" +room+ "/" + v);
        sgs.then(function (data) {
            if (data !== "Done") {
                alert("Es ist ein Fehler aufgetreten:" + data);
            }
            t.UpdateData();
        });

    };
    this.GetGroupScenarios = function () {
        if (GroupScenariousRendered === true) return true;
        var ggs = this.Send("GetGroupScenariosForRooms");
        ggs.then(function (data) {
            if (data === null) {
                window.setTimeout("aurora.GetGroupScenariosForRooms", 25000);
                return;
            }
            let aurora_Group = document.getElementById("Aurora_Group");
            let groupwrapper = document.createElement("DIV");
            groupwrapper.id = "GroupPowerScenarios";
            groupwrapper.classList.add("groupcontainer")
            aurora_Group.append(groupwrapper);
            let foundedrooms = Object.getOwnPropertyNames(data);
            for (var i = 0; i < foundedrooms.length; i++) {
                let room = foundedrooms[i];
                let roomlist = data[room];
                if (roomlist.length > 0) {
                    let group = document.createElement("DIV");
                    group.classList.add("groupscenario");
                    group.innerText = room;
                    groupwrapper.append(group);
                    for (var j = 0; j < roomlist.length; j++) {
                        let groupentries = document.createElement("DIV");
                        groupentries.innerText = roomlist[j];
                        groupentries.classList.add("groupscenarioentry");
                        groupentries.dataset.room = room;
                        group.append(groupentries);
                        groupentries.addEventListener("click", function (event) {
                            t.SetGroupScenario(event.target.innerText, event.target.dataset.room);
                        });
                    }
                }
            }
            GroupScenariousRendered = true;
        });
    }
    this.GetAurora = function (serial) {
        for (var i = 0; i < _data.length; i++) {
            if (_data[i].NewAurora === true || _data[i].nlj === null) continue;
            var s = _data[i].nlj.serialNo;
            if (s === serial) {
                return _data[i].nlj;
            }
        }
        return false;
    };
    this.SetPowerState = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (typeof v === "boolean" && v !== au.state.powerstate.value) {
            this.Send("SetPowerState/" + serial + "/" + v);
            au.state.powerstate.value = v;
            this.RenderAurora();
        }
    };
    this.SetHue = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (v !== au.state.hue.value && v >= au.state.hue.min && v <= au.state.hue.max) {
            this.Send("SetHue/" + serial + "/" + v);
            au.state.hue.value = v;
            this.RenderAurora();
        }
    };
    this.SetBrigthness = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (v !== au.state.brightness.value && v >= au.state.brightness.min && v <= au.state.brightness.max) {
            this.Send("SetBrightness/" + serial + "/" + v);
            au.state.brightness.value = v;
            this.RenderAurora();
        }
    };
    this.SetSaturation = function (v, serial) {
        var au = this.GetAurora(serial);
        if (au === false) return;
        if (v !== au.state.saturation.value && v >= au.state.saturation.min && v <= au.state.saturation.max) {
            this.Send("SetSaturation/" + serial + "/" + v);
            au.state.hue.value = v;
            this.RenderAurora();
        }
    };
    this.SetPower = function (serial) {
        var t = _PowerDom[serial].checked;
        this.SetPowerState(!t, serial);
    };
    this.SetSelectedScenario = function (v, serialNo) {
        var au = this.GetAurora(serialNo);
        if (au === false) return;
        au.effects.selected = v;
        if (au.state.powerstate.value !== true) {
            au.state.powerstate.value = true;
        }
        this.Send("SetSelectedScenario/" + serialNo + "/" + v).then(function () {
            t.ColorPickers[serialNo].color.hsv = { h: 0, s: 0, v: au.state.brightness.value };
            t.RenderAurora();
        }).catch(function (err) {
            console.log(err);
        });


        return;
    };
    this.ColorPickerInit = function (serial, aur) {
        var cp = new iro.ColorPicker('#ColorPicker_' + serial, { width: 160, color: { h: aur.state.hue.value, s: aur.state.saturation.value, v: aur.state.brightness.value } });
        if (typeof this.ColorPickers[serial] === "undefined") {
            this.ColorPickers[serial] = cp;
        }
        //color.setChannel('hsv', 'h', 255);
        cp.on('input:end', function () {
            // log the current color as a HEX string
            t.UpdateColor(serial);

        });
    };
    this.UpdateColor = function (serial) {
        console.log(serial);
        var aur = this.GetAurora(serial);
        var st = aur.state;
        var color = this.ColorPickers[serial].color.hsv;
        st.hue.value = color.h;
        st.saturation.value = color.s
        st.brightness.value = color.v
        this.Send("SetHSVColor/" + serial, color, "POST");


    }
    this.RenderAurora = function () {
        if (typeof _data === "undefined") {
            alert("Aurora ist nicht initialisiert");
            return false;
        }
        var room = "Alle";
        if (_newData === true) {
            for (var x = 0; x < _data.length; x++) {
                var faid = "new";
                if (typeof _data[x].room !== "undefined" && _data[x].room !== null && _data[x].room !== "") {
                    room = _data[x].room;
                }
                if (_data[x].newAurora === false) {
                    faid = _data[x].serialNo;
                }
                let aurlight = document.createElement("DIV");
                aurlight.id = "Aurora_" + faid;
                aurlight.classList.add("auroraContainer");
                let aurname = document.createElement("DIV");
                aurname.innerText = _data[x].name;
                aurname.classList.add("auroraName");
                let containerdom = document.createElement("DIV");
                containerdom.classList.add("container");
                let inputdom = document.createElement("input");
                inputdom.setAttribute("type", "checkbox");
                inputdom.setAttribute("onClick", _opjectname + '.SetPower(\'' + faid + '\')');
                inputdom.setAttribute("name", "power_" + faid);
                inputdom.id = "power_" + faid;
                inputdom.classList.add("powerCheck");
                inputdom.checked = true;
                let labeldom = document.createElement("label");
                labeldom.setAttribute("for", "power_" + faid);
                labeldom.classList.add("power");
                let span1 = document.createElement("span");
                let span2 = document.createElement("span");
                span1.classList.add("icon-off");
                span2.classList.add("light");
                labeldom.append(span1);
                labeldom.append(span2);
                containerdom.append(inputdom);
                containerdom.append(labeldom);
                let cp = document.createElement("DIV");
                cp.id = "ColorPicker_" + faid;
                let scen = document.createElement("DIV");
                scen.id = "Scenarios_" + faid;
                scen.setAttribute("data-serial", faid);
                scen.classList.add("scenarios");
                aurlight.append(aurname);
                aurlight.append(containerdom);
                aurlight.append(cp);
                aurlight.append(scen);
                auroraWrapper.append(aurlight);
                this.ColorPickerInit(faid, _data[x].nlj);
            }
            _newData = false;
        }
        if (_data.length > 1) {
            let ag = document.createElement("DIV");
            ag.id = "Aurora_Group";
            ag.classList.add("auroraContainer");

            for (var i = 0; i < _data.length; i++) {
                let room = _data[i].room;
                if (workedGroupPower.indexOf(room) != -1 || room === null) continue;

                workedGroupPower.push(room);
                let agname = document.createElement("DIV");
                agname.classList.add("auroraName");
                agname.innerText = room;
                let agon = document.createElement("DIV");
                agon.innerText = "Power On";
                agon.classList.add("groupcontainer");
                agon.id = "GroupPowerOn";
                agon.addEventListener("click", function () {
                    t.Send("SetGroupPowerState/" + room + "/true").then(function () {
                        t.UpdateData();
                    });
                })
                let agoff = document.createElement("DIV");
                agoff.innerText = "Power Off";
                agoff.classList.add("groupcontainer");
                agoff.id = "GroupPowerOff";
                agoff.addEventListener("click", function () {
                    t.Send("SetGroupPowerState/" + room + "/false").then(function () {
                        t.UpdateData();
                    });
                })
                ag.append(agname);
                ag.append(agon);
                ag.append(agoff);
            }
            
            auroraWrapper.append(ag);
        }

        if (_data.length > 1) {
            this.GetGroupScenarios();
        }
        for (var i = 0; i < _data.length; i++) {
            //Check new Aurora and continue
            if (_data[i].newAurora === true || _data[i].nlj === null) {
                continue;
            }
            var aid = _data[i].serialNo;
            //Power
            _PowerDom[aid] = document.getElementById("power_" + aid);
            if (_PowerDom[aid].checked !== !_data[i].nlj.state.powerstate.value) {
                _PowerDom[aid].checked = !_data[i].nlj.state.powerstate.value;
            }
            //Scenarios
            var effectlist = _data[i].nlj.effects.scenariosDetailed.animations;
            if (effectlist === 0) {
                alert("Keine Scenarien geliefert Aurora Serial:" + _data[i].serialNo);
                continue;
            }
            var sd = document.getElementById("Scenarios_" + aid);
            sd.innerHTML = "";
            for (var y = 0; y < effectlist.length; y++) {
                let item = effectlist[y];
                let aurscen = document.createElement("DIV");
                aurscen.setAttribute("data-Name", item.animName);
                aurscen.classList.add("aurorascenario");
                if (item.animName === _data[i].nlj.effects.selected) {
                    aurscen.classList.add(_SelectedScenarioClass);
                }
                aurscen.addEventListener("click", function (event) {
                    let d = event.target.dataset.name;
                    var serial = event.target.parentNode.dataset.serial;
                    t.SetSelectedScenario(d, serial);
                });
                if (item.pluginType === "rhythm") {
                    let rhythm = document.createElement("img");
                    rhythm.setAttribute("src", "/images/rhythm.jpg");
                    rhythm.classList.add("rhythmimage");
                    aurscen.append(rhythm);
                }
                aurscen.innerHTML += item.animName;
                sd.append(aurscen);
            }
            if (_data[i].nlj.effects.selected !== "*Solid*") {
                this.ColorPickers[aid].color.hsv = { h: 0, s: 0, v: _data[i].nlj.state.brightness.value }
            } else {
                this.ColorPickers[aid].color.hsv = { h: _data[i].nlj.state.hue.value, s: _data[i].nlj.state.saturation.value, v: _data[i].nlj.state.brightness.value }
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
        var request = this.Send("Get");
        request.then(function (data) {
            if (typeof _data === "undefined" || _data.length !== data.length) {
                _newData = true;
                auroraWrapper.innerHTML = "";
                _PowerDom = [];
            } else {
                _newData = false;
            }
            _data = data;
            t.d = data;
            t.RenderAurora();
            return true;
        }).catch(function (ex) {
            console.log(ex);
        });
    };
    this.Send = async function (url = '', data = {}, t = 'GET') {
        // Default options are marked with *
        if (typeof BasePath !== "undefined" && BasePath !== "" && BasePath !== null) {
            url = BasePath + url;
        }
        var fetchparams = {
            method: t, // *GET, POST, PUT, DELETE, etc.
            mode: 'cors', // no-cors, *cors, same-origin
            cache: 'no-cache', // *default, no-cache, reload, force-cache, only-if-cached
            credentials: 'same-origin', // include, *same-origin, omit
            headers: {
                'Content-Type': 'application/json'
            },
            redirect: 'follow', // manual, *follow, error
            referrerPolicy: 'no-referrer' // no-referrer, *no-referrer-when-downgrade, origin, origin-when-cross-origin, same-origin, strict-origin, strict-origin-when-cross-origin, unsafe-url
        };
        if (t === "POST") {
            fetchparams.body = JSON.stringify(data); // body data type must match "Content-Type" header
        }
        const response = await fetch(url, fetchparams);

        var res = await response.text(); //take text

        try {
            return JSON.parse(res);// parses JSON response into native JavaScript objects
        } catch (e) {
            return res;
        }
    }

}