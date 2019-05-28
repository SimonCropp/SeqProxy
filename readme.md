<!--
GENERATED FILE - DO NOT EDIT
This file was generated by [MarkdownSnippets](https://github.com/SimonCropp/MarkdownSnippets).
Source File: /mdsource/readme.source.md
To change this file edit the source file and then run MarkdownSnippets.
-->
# <img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/icon.png" height="30px"> SeqProxy

Enables writing seq logs by proxying requests through an ASP.NET Controller.


## Why

 * Avoid exposing the Seq API to the internet.
 * Leverage [Asp AUthentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.
 * Append [extra data](#extra-data) to log messages during server processing.


## The NuGet package [![NuGet Status](http://img.shields.io/nuget/v/SeqProxy.svg)](https://www.nuget.org/packages/SeqProxy/)

https://nuget.org/packages/SeqProxy/

    PM> Install-Package SeqProxy


## HTTP Format/Protocol

Format: [Serilog compact](https://github.com/serilog/serilog-formatting-compact).

Protocol: [Seq raw events](https://docs.datalust.co/docs/posting-raw-events).

Note that timestamp (`@t`) is optional when using this project. If it is not supplied the server timestamp will be used.


## Extra data

For every log entry written the following information is appended:

 * The current application name (as `AppName`) defined in code at startup.
 * The current application version (as `AppVersion`) defined in code at startup.
 * The server name (as `Server`) using `Environment.MachineName`.
 * All claims for the current User from `ControllerBase.User.Claims`.
 * The [user agent header](https://en.wikipedia.org/wiki/User_agent) as `UserAgent`.


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

<!-- snippet: ConfigureServices -->
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore();
    services.AddSeqWriter(
        seqUrl: "http://localhost:5341",
        appName: "Sample",
        appVersion: GetType().Assembly.GetName().Version);
}
```
<sup>[snippet source](/src/SampleWeb/Startup.cs#L14-L25)</sup>
<!-- endsnippet -->


### Implement a Controller

Add a new controller that overrides `BaseSeqController`.

<!-- snippet: SimpleController -->
```cs
public class SeqController :
    BaseSeqController
{
    public SeqController(SeqWriter seqWriter) :
        base(seqWriter)
    {
    }
}
```
<sup>[snippet source](/src/Tests/ControllerSamples.cs#L8-L17)</sup>
<!-- endsnippet -->


#### Authorization/Authentication

Adding authorization and authentication can be done with an [AuthorizeAttribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple).

<!-- snippet: AuthorizeController -->
```cs
[Authorize]
public class SeqController :
    BaseSeqController
{
    public SeqController(SeqWriter seqWriter) :
        base(seqWriter)
    {
    }
}
```
<sup>[snippet source](/src/Tests/ControllerSamples.cs#L46-L56)</sup>
<!-- endsnippet -->


### Method level attributes

Method level Asp attributes can by applied by overriding `BaseSeqController.Post`.

For example adding an [exception filter ](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters).

<!-- snippet: OverridePostController -->
```cs
public class SeqController :
    BaseSeqController
{
    public SeqController(SeqWriter seqWriter) :
        base(seqWriter)
    {
    }
    [CustomExceptionFilter]
    public override Task Post()
    {
        return base.Post();
    }
}
```
<sup>[snippet source](/src/Tests/ControllerSamples.cs#L22-L37)</sup>
<!-- endsnippet -->


## Client Side Usage


### Using raw JavaScript

Writing to Seq can be done using a HTTP post:

<!-- snippet: LogRawJs -->
```js
function LogRawJs() {
    const textInput = document.getElementById('textInput').value;
    const postSettings = {
        method: 'POST',
        credentials: 'include',
        mode: 'cors',
        body: `{'@mt':'RawJs input: {Text}','Text':'${textInput}'}`
    };

    return fetch('/api/events/raw', postSettings);
}
```
<sup>[snippet source](/src/SampleWeb/sample.js#L21-L33)</sup>
<!-- endsnippet -->


### Using Structured-Log

[structured-log](https://github.com/structured-log/structured-log/) is a structured logging framework for JavaScript, inspired by Serilog.

In combination with [structured-log-seq-sink](https://github.com/Wedvich/structured-log-seq-sink) it can be used to write to Seq

To use this approach:


#### Include the libraries

Install both [structured-log npm](https://www.npmjs.com/package/structured-log) and [structured-log-seq-sink npm](https://www.npmjs.com/package/structured-log-seq-sink). Or include them from [jsDelivr](https://www.jsdelivr.com/):

<!-- snippet: StructuredLogInclude -->
```html
<script src='https://cdn.jsdelivr.net/npm/structured-log/dist/structured-log.js'></script>
<script src='https://cdn.jsdelivr.net/npm/structured-log-seq-sink/dist/structured-log-seq-sink.js'></script>
```
<sup>[snippet source](/src/SampleWeb/sample.html#L4-L7)</sup>
<!-- endsnippet -->


#### Configure the log

<!-- snippet: StructuredLogConfig -->
```js
var levelSwitch = new structuredLog.DynamicLevelSwitch('info');
const log = structuredLog.configure()
    .writeTo(new structuredLog.ConsoleSink())
    .minLevel(levelSwitch)
    .writeTo(SeqSink({
        url: "http://localhost:5000",
        compact: true,
        levelSwitch: levelSwitch
    }))
    .create();
```
<sup>[snippet source](/src/SampleWeb/sample.js#L1-L12)</sup>
<!-- endsnippet -->


#### Write a log message

<!-- snippet: StructuredLog -->
```js
function LogStructured() {
    const textInput = document.getElementById('textInput').value;
    log.info('StructuredLog input: {Text}', textInput);
}
```
<sup>[snippet source](/src/SampleWeb/sample.js#L14-L19)</sup>
<!-- endsnippet -->


## Icon

<a href="http://thenounproject.com/term/robot/883226/">Robot</a> designed by <a href="https://thenounproject.com/maxim221/">Maxim Kulikov</a> from The Noun Project
