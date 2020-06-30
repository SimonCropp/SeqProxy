# <img src="/src/icon.png" height="30px"> SeqProxy

[![Build status](https://ci.appveyor.com/api/projects/status/7996jd4uoooy5qy2/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/SeqProxy)
[![NuGet Status](https://img.shields.io/nuget/v/SeqProxy.svg)](https://www.nuget.org/packages/SeqProxy/)

Enables writing seq logs by proxying requests through an ASP.NET Controller or Middleware.

Support is available via a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-seqproxy?utm_source=nuget-seqproxy&utm_medium=referral&utm_campaign=enterprise).


## Why

 * Avoid exposing the Seq API to the internet.
 * Leverage [Asp Authentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.
 * Append [extra data](#extra-data) to log messages during server processing.

toc


## NuGet package

https://nuget.org/packages/SeqProxy/


## HTTP Format/Protocol

Format: [Serilog compact](https://github.com/serilog/serilog-formatting-compact).

Protocol: [Seq raw events](https://docs.datalust.co/docs/posting-raw-events).

Note that timestamp (`@t`) is optional when using this project. If it is not supplied the server timestamp will be used.


## Extra data

For every log entry written the following information is appended:

 * The current application name (as `Application`) defined in code at startup.
 * The current application version (as `ApplicationVersion`) defined in code at startup.
 * The server name (as `Server`) using `Environment.MachineName`.
 * All claims for the current User from `ControllerBase.User.Claims`.
 * The [user-agent header](https://en.wikipedia.org/wiki/User_agent) as `UserAgent`.
 * The [referer header](https://en.wikipedia.org/wiki/HTTP_referer) as `Referrer`.

<img src="/src/extraData.png">


### SeqProxyId

SeqProxyId is a tick based timestamp to help correlating a front-end error with a Seq log entry.

It is appended to every Seq log entry and returned as a header to HTTP response.

The id is generated using the following:

snippet: BuildId

Which generates a string of the form `8e434f861302`. This string can then be given to a user as a error correlation id.

Then the log entry can be accessed using a Seq filter.

`http://seqServer/#/events?filter=SeqProxyId%3D'39f616eeb2e3'`


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

snippet: ConfigureServices

There are several optional parameters:

snippet: ConfigureServicesFull

 * `application` defaults to `Assembly.GetCallingAssembly().GetName().Name`.
 * `applicationVersion` defaults to `Assembly.GetCallingAssembly().GetName().Version`.
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

`BaseSeqController` is an implementation of `ControllerBase` that provides a HTTP post and some basic routing.

snippet: BaseSeqController.cs

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

<img src="/src/omitFromMessage.png">


## Security contact information

To report a security vulnerability, use the [Tidelift security contact](https://tidelift.com/security). Tidelift will coordinate the fix and disclosure.


## Icon

[Robot](https://thenounproject.com/term/robot/883226/) designed by [Maxim Kulikov](https://thenounproject.com/maxim221) from [The Noun Project](https://thenounproject.com).