﻿var self = this;

var jsonInput = @Html.Raw(Model.ReturnedValue);
var mnemonics = Object.keys(jsonInput);
var numberOfMnemonics = mnemonics.length;

var boxStrokeWidth = 2;
var pathStrokeWidth = 1;
var logLegendHeight = 50;
var logLegendWidth = 200;
var logWidth = logLegendWidth;
var logHeight = 900;
var logBoxVerticalOffset = logLegendHeight;
var logBoxHorizontalOffset = logLegendWidth;
var wellOffsetY = 50;
var wellWidth = (numberOfMnemonics - 1) * logWidth;
var wellHeight = logLegendHeight + logHeight;

var svg = d3.select("body").append("svg").attr("width", wellWidth).attr("height", wellHeight);

// Set up the depth axis log data once as it will be used for each other log in the well
var depthData = jsonInput[mnemonics[0]];
var y = d3.scale.linear().range([0, logHeight]).domain(d3.extent(depthData));
var yAxis = d3.svg.axis().scale(y).orient("left").ticks(5);

var wellBoxData = [
    { "x": 0, "y": 0 },
    { "x": wellWidth, "y": 0 },
    { "x": wellWidth, "y": wellHeight },
    { "x": 0, "y": wellHeight },
    { "x": 0, "y": 0 }
];

var logLegendBoxData = [
    { "x": 0, "y": 0 },
    { "x": logLegendWidth, "y": 0 },
    { "x": logLegendWidth, "y": logLegendHeight },
    { "x": 0, "y": logLegendHeight },
    { "x": 0, "y": 0 }
];

var logBoxData = [
    { "x": 0, "y": 0 },
    { "x": logWidth, "y": 0 },
    { "x": logWidth, "y": logHeight },
    { "x": 0, "y": logHeight },
    { "x": 0, "y": 0 }
];

var box = d3.svg.line()
    .x(function (d) { return d.x })
    .y(function (d) { return d.y; })
    .interpolate("linear");

var valueLine = d3.svg.line()
    .x(function (d) { return x(d.x) })
    .y(function (d) { return y(d.y) })
    .interpolate("linear");

var frameIndex = 0;

function getLogData(arrayID, arrayText) {

    var newLogData = '[ ';
    //should arrayID length equal arrayText length and both against null
    if (arrayID != null && arrayText != null && arrayID.length == arrayText.length) {
        for (var i = 0; i < arrayID.length; i++) {
            if (i < arrayID.length - 1) {
                newLogData += '{ "x" : ' + arrayText[i] + ', "y" : ' + arrayID[i] + ' }, ';
            } else {
                newLogData += '{ "x" : ' + arrayText[i] + ', "y" : ' + arrayID[i] + ' } ';
            }
        }
    }
    newLogData += ']';
    return JSON.parse(newLogData);
}

for (var i = 1; i < numberOfMnemonics; i++) {
    var loopself = this;
    var translationX = logLegendWidth * (i - 1);
    var legendTranslationDistanceString = ("translate(" + translationX.toString() + ", 0)");
    var logTranslationDistanceString =
        ("translate(" + translationX.toString() + "," + logLegendHeight.toString() + ")");
    svg.append("path")
        .attr("d", box(logLegendBoxData))
        .attr("stroke", "gray")
        .attr("stroke-width", pathStrokeWidth)
        .attr("fill", "none")
        .attr("transform", legendTranslationDistanceString);
    svg.append("text")
        .attr("stroke", "black")
        .attr("stroke-width", pathStrokeWidth)
        .attr("x", 5)
        .attr("y", logLegendHeight / 2)
        .attr("dy", ".35em")
        .text(mnemonics[i])
        .attr("transform", legendTranslationDistanceString);
    svg.append("path")
        .attr("d", box(logBoxData))
        .attr("stroke", "gray")
        .attr("stroke-width", pathStrokeWidth)
        .attr("fill", "none")
        .attr("transform", logTranslationDistanceString);

    var inputLogData = jsonInput[mnemonics[i]];
    var x = d3.scale.linear().range([0, logWidth]).domain(d3.extent(inputLogData));
    var xAxis = d3.svg.axis().scale(x).orient("bottom").ticks(5);
    var logData = getLogData(depthData, inputLogData);

    svg.append("path")
        .attr("d", valueLine(logData))
        .attr("stroke", "gray")
        .attr("stroke-width", pathStrokeWidth)
        .attr("transform", logTranslationDistanceString)
        .attr("fill", "none");
}
//svg.append("path")
//    .attr("d", box(wellBoxData))
//    .attr("stroke", "red")
//    .attr("stroke-width", boxStrokeWidth*2)
//    .attr("fill", "none");
//svg.append("path")
//    .attr("class", "y axis")
//    .attr("stroke", "green")
//    .attr("stroke-width", pathStrokeWidth)
//    .attr("transform", logTranslationDistanceString)
//    .call(yAxis);
//svg.append("path")
//    .attr("class", "x axis")
//    .attr("stroke", "yellow")
//    .attr("stroke-width", pathStrokeWidth)
//    .attr("transform", logTranslationDistanceString)
//    .call(xAxis);
