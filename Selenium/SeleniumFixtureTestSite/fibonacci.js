"use strict";

function isNoNumber(factor)
{
    return isNaN(parseInt(factor));
}

function Fibonacci(factor)
{
    if (isNoNumber(factor)) 
    {
        return "Input should be numerical";
    }

    var maxInt = Math.pow(2,32);
    if (factor < 0)
    {
        return "Input should be 0 or positive";
    }

    if (factor < 2) { return factor; }

    var previousNumber = 1;
    var currentNumber = 1;

    for (var i = 2; i < factor; i++)
    {
        if (maxInt - currentNumber < previousNumber) return "Overflow";
        var nextNumber = previousNumber + currentNumber;
        previousNumber = currentNumber;
        currentNumber = nextNumber;
    }
    return currentNumber;
}
