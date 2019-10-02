function errorMessage(messageText) {
    message(messageText, "fail");
}

function successMessage(messageText) {
    message(messageText, "success");
}

function message(messageText, messageType) {
    var state = document.getElementById("status");
    state.className = messageType;
    state.innerHTML = messageText;
}

function allowDrop(ev) {
    ev.preventDefault();
}


function changeParagraph() {
    document.getElementById("paragraph").innerHTML = "Changed Paragraph Text after Double Click";
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

function getJavaScriptTestTable(target) {
    var numbers = [-1, 0, 1, 10, 47, 48, 1000, "aq"];
    var result = "";
    for (var i = 0; i < numbers.length; i++) {
        result += "<tr><td>" + numbers[i] + "</td><td>" + Fibonacci(numbers[i]) + "</td><td>" + isNoNumber(numbers[i]) + "</td></tr>";
    }
    var javaScriptDiv = document.getElementById(target);
    javaScriptDiv.innerHTML = result;
}

function changeLightbulbImage() {
    var image = document.getElementById("imageLightbulb");
    if (image.src.match("bulbon")) {
        image.src = "pic_bulboff.gif";
        image.alt = "Light bulb off";
    } else {
        image.src = "pic_bulbon.gif";
        image.alt = "Light bulb on";
    }
}

function hoverLightbulb(yes) {
    successMessage(yes ? "Hovering over image" : "OK");
}

function asyncDataLoad(callback, element) {
    var _ = setTimeout(function () {
        var data = [0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89];
        callback(data, element);
    }, 1000);
}

function writeData(myData, element) {
    document.getElementById(element).innerHTML = myData;
    successMessage("Data load completed");
}

function upload(event) {
    var input = event.target;
    var output = document.getElementById("fileContent");
    var reader = new FileReader();
    reader.onload = function (loadEvent) {
        output.innerHTML = loadEvent.target.result;
    };
    reader.readAsText(input.files[0]);
}

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

function setLocalData() {
    localStorage.setItem("lastName", "Smith");
    localStorage.setItem("firstName", "John");
}

function clearLocalData() {
    localStorage.clear();
}

function getLocalData() {
    var textField = document.getElementById("localData");
    textField.value = localStorage.getItem("firstName");
}

function setSessionData() {
    sessionStorage.setItem("lastName", "Jones");
    sessionStorage.setItem("firstName", "Penelope");
}

function clearSessionData() {
    sessionStorage.clear();
}

function getSessionData() {
    var textField = document.getElementById("sessionData");
    textField.value = sessionStorage.getItem("firstName");
}

function updateMonth(value) {
    successMessage(value);
}

function toggleHideButtonClick() {
    var _ = setTimeout(function () {
        var button = document.getElementById("hiddenButton");
        if (button.style.display === "none") {
            button.style.display = "inline";
            button.innerText = "Unhidden button";
        } else {
            button.style.display = "none";
            button.innerText = "Hidden button";
        }
    }, document.getElementById("delay").value);    
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
    }, document.getElementById("delay").value);
}

