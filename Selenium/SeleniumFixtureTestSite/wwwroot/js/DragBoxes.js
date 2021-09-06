// Copyright 2015-2021 Rik Essenius
//
//   Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file 
//   except in compliance with the License. You may obtain a copy of the License at
//
//       http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software distributed under the License 
//   is distributed on an "AS IS" BASIS WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and limitations under the License.
//
//   Inspired by a demo at http://www.html5tuts.co.uk/ which got discontinued and was licensed under CreativeCommons 2.0,
//   https://creativecommons.org/licenses/by-nc/2.0/uk/

"use strict";

var drageSourceElement = null;
var boxesList = null;

function handleDragStart(e) {
    e.dataTransfer.effectAllowed = "move";
    e.dataTransfer.setData("text/html", this.innerHTML);
    drageSourceElement = this;
    this.style.opacity = "0.5";
    this.addClassName("moving");
}

function handleDragOver(e) {
    if (e.preventDefault) {
        e.preventDefault();
    }
    e.dataTransfer.dropEffect = "move";
    return false;
}

function handleDragEnter(e) { this.addClassName("over"); }

function handleDragLeave(e) { this.removeClassName("over"); };

function handleDrop(e) {
    if (e.stopPropagation) {
        e.stopPropagation();
    }
    if (drageSourceElement !== this) {
        drageSourceElement.innerHTML = this.innerHTML;
        this.innerHTML = e.dataTransfer.getData("text/html");
    }
    return false;
};

function handleDragEnd(e) {
    this.style.opacity = "1";

    [].forEach.call(boxesList,
        function(box) {
            box.removeClassName("over");
            box.removeClassName("moving");
        });
};

function initBoxes() {
    var section = document.getElementById("sectionDragAndDropBoxes");
    boxesList = section.querySelectorAll(".box");

    [].forEach.call(boxesList,
        function(box) {
            box.setAttribute("draggable", "true");
            box.addEventListener("dragstart", handleDragStart, false);
            box.addEventListener("dragenter", handleDragEnter, false);
            box.addEventListener("dragover", handleDragOver, false);
            box.addEventListener("dragleave", handleDragLeave, false);
            box.addEventListener("drop", handleDrop, false);
            box.addEventListener("dragend", handleDragEnd, false);
        });
}