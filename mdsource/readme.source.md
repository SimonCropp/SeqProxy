# <img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/icon.png" height="40px"> SeqProxy

Enables writing seq logs by proxying requests through an ASP.NET Controller.


## Why

 * Avoid exposing the Seq API to the internet.
 * Leverage [Asp Authentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.
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
 * The [user-agent header](https://en.wikipedia.org/wiki/User_agent) as `UserAgent`.
 * The [referer header](https://en.wikipedia.org/wiki/HTTP_referer) as `Referrer`.


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

snippet: ConfigureServices

There are several optional parameters:

snippet: ConfigureServicesFull


 * `appName` defaults to `Assembly.GetCallingAssembly().GetName().Name`.
 * `appVersion` defaults to `Assembly.GetCallingAssembly().GetName().Version`.
 * `scrubClaimType` is used to clean up claimtype strings. For example [ClaimTypes.Email](https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.claims.claimtypes.email?System_IdentityModel_Claims_ClaimTypes_Email) is `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress`, but when recording to Seq the value `emailaddress` is sufficient. Defaults to `DefaultClaimTypeScrubber.Scrub` to get the string after the last `/`.

snippet: DefaultClaimTypeScrubber.cs


### Implement a Controller

Add a new controller that overrides `BaseSeqController`.

snippet: SimpleController


#### Authorization/Authentication

Adding authorization and authentication can be done with an [AuthorizeAttribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple).

snippet: AuthorizeController


### Method level attributes

Method level Asp attributes can by applied by overriding `BaseSeqController.Post`.

For example adding an [exception filter ](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters).

snippet: OverridePostController


## Client Side Usage


### Using raw JavaScript

Writing to Seq can be done using a HTTP post:

snippet: LogRawJs


### Using Structured-Log

[structured-log](https://github.com/structured-log/structured-log/) is a structured logging framework for JavaScript, inspired by Serilog.

In combination with [structured-log-seq-sink](https://github.com/Wedvich/structured-log-seq-sink) it can be used to write to Seq

To use this approach:


#### Include the libraries

Install both [structured-log npm](https://www.npmjs.com/package/structured-log) and [structured-log-seq-sink npm](https://www.npmjs.com/package/structured-log-seq-sink). Or include them from [jsDelivr](https://www.jsdelivr.com/):

snippet: StructuredLogInclude


#### Configure the log

snippet: StructuredLogConfig


#### Write a log message

snippet: StructuredLog


## Icon

<a href="http://thenounproject.com/term/robot/883226/">Robot</a> designed by <a href="https://thenounproject.com/maxim221/">Maxim Kulikov</a> from The Noun Project