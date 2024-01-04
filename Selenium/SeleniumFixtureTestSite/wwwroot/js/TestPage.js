// Copyright 2015-2024 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.

"use strict";

// Async Loading

function asyncDataLoad(callback, element) {
    var _ = setTimeout(function() {
            successMessage("called load");
            var data = [0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89];
            callback(data, element);
        },
        1000);
}

function ayncLoad(element) {
    asyncDataLoad(writeData, element);
}

// Browser Data

function clearLocalData() {
    localStorage.clear();
}

function clearSessionData() {
    sessionStorage.clear();
}

function getLocalData() {
    var textField = document.getElementById("localData");
    textField.value = localStorage.getItem("firstName");
}

function getSessionData() {
    var textField = document.getElementById("sessionData");
    textField.value = sessionStorage.getItem("firstName");
}

function setLocalData() {
    localStorage.setItem("lastName", "Smith");
    localStorage.setItem("firstName", "John");
}

function setSessionData() {
    sessionStorage.setItem("lastName", "Jones");
    sessionStorage.setItem("firstName", "Penelope");
}

// Dialogs

function showConfirmDialog() {
    if (window.confirm("Press OK or Cancel")) {
        successMessage("You pressed OK");
    } else {
        successMessage("You pressed Cancel");
    }
}

function showPromptDialog() {
    var promptDialog = window.prompt("Please enter a response", "sure");
    if (promptDialog === null) {
        successMessage("You pressed Cancel");
    } else {
        successMessage("You returned: " + promptDialog);
    }
}

// Drag & Drop

function allowDrop(ev) {
    ev.preventDefault();
}

function drag(ev) {
    ev.dataTransfer.setData("Text", ev.target.id);
}

function drop(ev) {
    ev.preventDefault();
    var data = ev.dataTransfer.getData("Text");
    if (data !== "") {
        ev.target.appendChild(document.getElementById(data));
        successMessage("OK");
    } else {
        errorMessage("could not find item to drop");
    }
}

// Miscellaneous

function changeLightbulbImage() {
    var image = document.getElementById("imageLightbulb");
    if (image.src.match("bulbon")) {
        image.src = "images/pic_bulboff.gif";
        image.alt = "Light bulb off";
    } else {
        image.src = "images/pic_bulbon.gif";
        image.alt = "Light bulb on";
    }
}

function changeParagraph() {
    document.getElementById("paragraph").innerHTML = "Changed Paragraph Text after Double Click";
}

function hoverLightbulb(yes) {
    successMessage(yes ? "Hovering over image" : "OK");
}

function toggleDisabledButtonClick() {
    var _ = setTimeout(function() {
            var button = document.getElementById("disabledButton");
            if (button.disabled) {
                button.removeAttribute("disabled");
                button.innerText = "Enabled button";
            } else {
                button.disabled = "disabled";
                button.innerText = "Disabled button";
            }
        },
        document.getElementById("delay").value);
}

function toggleHideButtonClick() {
    var _ = setTimeout(function() {
            var button = document.getElementById("hiddenButton");
            if (button.style.display === "none") {
                button.style.display = "inline";
                button.innerText = "Unhidden button";
            } else {
                button.style.display = "none";
                button.innerText = "Hidden button";
            }
        },
        document.getElementById("delay").value);
}

function updateMonth(value) {
    successMessage(value);
}

function writeData(myData, element) {
    document.getElementById(element).innerHTML = myData;
    successMessage("Data load completed");
}

// Status Message 

function errorMessage(messageText) {
    message(messageText, "fail");
}

function message(messageText, messageType) {
    var state = document.getElementById("status");
    state.className = messageType;
    state.innerHTML = messageText;
}

function successMessage(messageText) {
    message(messageText, "success");
}

// Test Table

function getJavaScriptTestTable(target) {
    var numbers = [-1, 0, 1, 10, 47, 48, 1000, "aq"];
    var result = "";
    for (var i = 0; i < numbers.length; i++) {
        result += "<tr><td>" +
            numbers[i] +
            "</td><td>" +
            Fibonacci(numbers[i]) +
            "</td><td>" +
            isNoNumber(numbers[i]) +
            "</td></tr>";
    }
    var javaScriptDiv = document.getElementById(target);
    javaScriptDiv.innerHTML = result;
}

// Upload

function initFileDrop(dropTarget) {
    var holder = document.getElementById(dropTarget);
    if (typeof window.FileReader === "undefined") {
        errorMessage("File Reader not available");
    } else {
        successMessage("FileReader available");
    }

    holder.ondragover = function() {
        this.className = "hover";
        return false;
    };
    holder.ondragend = function() {
        this.className = "";
        return false;
    };
    holder.ondrop = function(e) {
        this.className = "";
        e.preventDefault();
        var file = e.dataTransfer.files[0],
            reader = new FileReader();
        if (file === undefined) {
            errorMessage("Could not identify file to upload");
        } else {
            reader.onload = function(event) {
                holder.style.background = "url(" + event.target.result + ") no-repeat center";
            };
            reader.readAsDataURL(file);
            successMessage("OK");
        }
        return false;
    };
}

function upload(event) {
    var input = event.target;
    var output = document.getElementById("fileContent");
    var reader = new FileReader();
    reader.onload = function(loadEvent) {
        output.innerHTML = loadEvent.target.result;
    };
    reader.readAsText(input.files[0]);
}