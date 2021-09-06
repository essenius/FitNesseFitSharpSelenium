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

"use strict";

function isNoNumber(factor) {
    return isNaN(parseInt(factor));
}

function Fibonacci(factor) {
    if (isNoNumber(factor)) {
        return "Input should be numerical";
    }

    var maxInt = Math.pow(2, 32);
    if (factor < 0) {
        return "Input should be 0 or positive";
    }

    if (factor < 2) {
        return factor;
    }

    var previousNumber = 1;
    var currentNumber = 1;

    for (var i = 2; i < factor; i++) {
        if (maxInt - currentNumber < previousNumber) return "Overflow";
        var nextNumber = previousNumber + currentNumber;
        previousNumber = currentNumber;
        currentNumber = nextNumber;
    }
    return currentNumber;
}