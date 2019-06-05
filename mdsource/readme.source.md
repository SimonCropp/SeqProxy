# <img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/icon.png" height="40px"> SeqProxy

Enables writing seq logs by proxying requests through an ASP.NET Controller or Middleware.


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

<img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/extraData.png">


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


### Add HTTP handling

There are two approaches to handling the HTTP containing log events. Using a Middleware and using a Controller.


#### Using a Middleware

Using a [Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) is done by calling `SeqWriterConfig.UseSeq` in `Startup.Configure(IApplicationBuilder builder)`:

snippet: ConfigureBuilder


##### Authorization

Authorization in the middleware can bu done by using `useAuthorizationService = true` in `UseSeq`.

snippet: StartupWithAuth

This then uses [IAuthorizationService](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased) to verify the request:

snippet: HandleWithAuth


#### Using a Controller

Add a new [controller](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) that overrides `BaseSeqController`.

snippet: SimpleController


##### Authorization/Authentication

Adding authorization and authentication can be done with an [AuthorizeAttribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple).

snippet: AuthorizeController


##### Method level attributes

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


#### Including data but omitting from the message template

When using structured-log, data not included in the message template will be named with a convention of `a+counter`. So for example if the following is logged:

```
log.info('The text: {Text}', text, "OtherData");
```

Then `OtherData` would be written to Seq with the property name `a1`.

To work around this:

Include a filter that replaces a known token name (in this case `{@Properties}`):

snippet: StructuredLogConfigExtraProp

Include that token name in the message template, and then include an object at the same position in the log parameters:

snippet: StructuredLogWithExtraProps

Then a destructured property will be written to Seq.

<img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/omitFromMessage.png">


## Icon

<a href="http://thenounproject.com/term/robot/883226/">Robot</a> designed by <a href="https://thenounproject.com/maxim221/">Maxim Kulikov</a> from The Noun Project