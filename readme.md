# <img src="/src/icon.png" height="30px"> SeqProxy

[![Build status](https://ci.appveyor.com/api/projects/status/7996jd4uoooy5qy2/branch/master?svg=true)](https://ci.appveyor.com/project/SimonCropp/SeqProxy)
[![NuGet Status](https://img.shields.io/nuget/v/SeqProxy.svg)](https://www.nuget.org/packages/SeqProxy/)

Enables writing [Seq](https://datalust.co/seq) logs by proxying requests through an ASP.NET Controller or Middleware.

Support is available via a [Tidelift Subscription](https://tidelift.com/subscription/pkg/nuget-seqproxy?utm_source=nuget-seqproxy&utm_medium=referral&utm_campaign=enterprise).


## Why

 * Avoid exposing the Seq API to the internet.
 * Leverage [Asp Authentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.
 * Append [extra data](#extra-data) to log messages during server processing.

<!-- toc -->
## Contents

  * [HTTP Format/Protocol](#http-formatprotocol)
  * [Extra data](#extra-data)
    * [SeqProxyId](#seqproxyid)
  * [Usage](#usage)
    * [Enable in Startup](#enable-in-startup)
    * [Add HTTP handling](#add-http-handling)
  * [Client Side Usage](#client-side-usage)
    * [Using raw JavaScript](#using-raw-javascript)
    * [Using Structured-Log](#using-structured-log)
  * [Security contact information](#security-contact-information)<!-- endToc -->


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

<!-- snippet: BuildId -->
<a id='d4391eea'></a>
```cs
var now = DateTime.UtcNow;
var startOfYear = new DateTime(now.Year, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
var ticks = now.Ticks - startOfYear.Ticks;
var id = ticks.ToString("x");
```
<sup><a href='/src/SeqProxy/SeqWriter.cs#L99-L106' title='Snippet source file'>snippet source</a> | <a href='#d4391eea' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which generates a string of the form `8e434f861302`. The current year is trimmed to shorten the id and under the assumption that retention policy is not longer than 12 months. There is a small chance of collisions, but given the use-case (error correlation), this should not impact the ability to find the correct error. This string can then be given to a user as a error correlation id.

Then the log entry can be accessed using a Seq filter.

`http://seqServer/#/events?filter=SeqProxyId%3D'39f616eeb2e3'`


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

<!-- snippet: ConfigureServices -->
<a id='7f21cb3f'></a>
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore(option => option.EnableEndpointRouting = false);
    services.AddSeqWriter(seqUrl: "http://localhost:5341");
}
```
<sup><a href='/src/SampleWeb/Startup.cs#L14-L22' title='Snippet source file'>snippet source</a> | <a href='#7f21cb3f' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There are several optional parameters:

<!-- snippet: ConfigureServicesFull -->
<a id='97dd0adb'></a>
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore();
    services.AddSeqWriter(
        seqUrl: "http://localhost:5341",
        apiKey: "TheApiKey",
        application: "MyAppName",
        appVersion: new Version(1, 2),
        scrubClaimType: claimType => claimType.Split("/").Last());
}
```
<sup><a href='/src/Tests/FullStartupConfig.cs#L7-L20' title='Snippet source file'>snippet source</a> | <a href='#97dd0adb' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

 * `application` defaults to `Assembly.GetCallingAssembly().GetName().Name`.
 * `applicationVersion` defaults to `Assembly.GetCallingAssembly().GetName().Version`.
 * `scrubClaimType` is used to clean up claimtype strings. For example [ClaimTypes.Email](https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.claims.claimtypes.email?System_IdentityModel_Claims_ClaimTypes_Email) is `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress`, but when recording to Seq the value `emailaddress` is sufficient. Defaults to `DefaultClaimTypeScrubber.Scrub` to get the string after the last `/`.

<!-- snippet: DefaultClaimTypeScrubber.cs -->
<a id='8a64f3bc'></a>
```cs
namespace SeqProxy
{
    /// <summary>
    /// Used for scrubbing claims when no other scrubber is defined.
    /// </summary>
    public static class DefaultClaimTypeScrubber
    {
        /// <summary>
        /// Get the string after the last /.
        /// </summary>
        public static string Scrub(string claimType)
        {
            Guard.AgainstNullOrEmpty(claimType, nameof(claimType));
            var lastIndexOf = claimType.LastIndexOf("/");
            if (lastIndexOf == -1)
            {
                return claimType;
            }

            return claimType.Substring(lastIndexOf + 1);
        }
    }
}
```
<sup><a href='/src/SeqProxy/DefaultClaimTypeScrubber.cs#L1-L23' title='Snippet source file'>snippet source</a> | <a href='#8a64f3bc' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Add HTTP handling

There are two approaches to handling the HTTP containing log events. Using a Middleware and using a Controller.


#### Using a Middleware

Using a [Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) is done by calling `SeqWriterConfig.UseSeq` in `Startup.Configure(IApplicationBuilder builder)`:

<!-- snippet: ConfigureBuilder -->
<a id='b7fbdaaa'></a>
```cs
public void Configure(IApplicationBuilder builder)
{
    builder.UseSeq();
```
<sup><a href='/src/SampleWeb/Startup.cs#L24-L29' title='Snippet source file'>snippet source</a> | <a href='#b7fbdaaa' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Authorization

Authorization in the middleware can bu done by using `useAuthorizationService = true` in `UseSeq`.

<!-- snippet: StartupWithAuth -->
<a id='894b7111'></a>
```cs
public void Configure(IApplicationBuilder builder)
{
    builder.UseSeq(useAuthorizationService: true);
```
<sup><a href='/src/Tests/StartupWithAuth.cs#L6-L11' title='Snippet source file'>snippet source</a> | <a href='#894b7111' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This then uses [IAuthorizationService](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased) to verify the request:

<!-- snippet: HandleWithAuth -->
<a id='0d357e4f'></a>
```cs
async Task HandleWithAuth(
    HttpContext context,
    IAuthorizationService authService)
{
    var user = context.User;
    var authResult = await authService.AuthorizeAsync(user, null, "SeqLog");

    if (!authResult.Succeeded)
    {
        await context.ChallengeAsync();
        return;
    }

    await seqWriter.Handle(
        user,
        context.Request,
        context.Response,
        context.RequestAborted);
}
```
<sup><a href='/src/SeqProxy/SeqMiddlewareWithAuth.cs#L37-L59' title='Snippet source file'>snippet source</a> | <a href='#0d357e4f' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Using a Controller

`BaseSeqController` is an implementation of `ControllerBase` that provides a HTTP post and some basic routing.

<!-- snippet: BaseSeqController.cs -->
<a id='3f5c15fd'></a>
```cs
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SeqProxy
{
    /// <summary>
    /// An implementation of <see cref="ControllerBase"/> that provides a http post and some basic routing.
    /// </summary>
    [Route("/api/events/raw")]
    [Route("/seq")]
    [ApiController]
    public abstract class BaseSeqController :
        ControllerBase
    {
        SeqWriter seqWriter;

        /// <summary>
        /// Initializes a new instance of <see cref="BaseSeqController"/>
        /// </summary>
        protected BaseSeqController(SeqWriter seqWriter)
        {
            Guard.AgainstNull(seqWriter, nameof(seqWriter));
            this.seqWriter = seqWriter;
        }

        /// <summary>
        /// Handles log events via a HTTP post.
        /// </summary>
        [HttpPost]
        public virtual Task Post()
        {
            return seqWriter.Handle(User, Request, Response, HttpContext.RequestAborted);
        }
    }
}
```
<sup><a href='/src/SeqProxy/BaseSeqController.cs#L1-L35' title='Snippet source file'>snippet source</a> | <a href='#3f5c15fd' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Add a new [controller](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) that overrides `BaseSeqController`.

<!-- snippet: SimpleController -->
<a id='343240c7'></a>
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
<sup><a href='/src/Tests/ControllerSamples.cs#L8-L17' title='Snippet source file'>snippet source</a> | <a href='#343240c7' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Authorization/Authentication

Adding authorization and authentication can be done with an [AuthorizeAttribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple).

<!-- snippet: AuthorizeController -->
<a id='48c664da'></a>
```cs
[Authorize]
public class SeqController :
    BaseSeqController
```
<sup><a href='/src/Tests/ControllerSamples.cs#L46-L50' title='Snippet source file'>snippet source</a> | <a href='#48c664da' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Method level attributes

Method level Asp attributes can by applied by overriding `BaseSeqController.Post`.

For example adding an [exception filter ](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters).

<!-- snippet: OverridePostController -->
<a id='876f0f82'></a>
```cs
public class SeqController :
    BaseSeqController
{
    [CustomExceptionFilter]
    public override Task Post()
    {
        return base.Post();
    }
```
<sup><a href='/src/Tests/ControllerSamples.cs#L22-L31' title='Snippet source file'>snippet source</a> | <a href='#876f0f82' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Client Side Usage


### Using raw JavaScript

Writing to Seq can be done using a HTTP post:

<!-- snippet: LogRawJs -->
<a id='b9f4e7ba'></a>
```js
function LogRawJs(text) {
    const postSettings = {
        method: 'POST',
        credentials: 'include',
        body: `{'@mt':'RawJs input: {Text}','Text':'${text}'}`
    };

    return fetch('/api/events/raw', postSettings);
}
```
<sup><a href='/src/SampleWeb/sample.js#L59-L69' title='Snippet source file'>snippet source</a> | <a href='#b9f4e7ba' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Using Structured-Log

[structured-log](https://github.com/structured-log/structured-log/) is a structured logging framework for JavaScript, inspired by Serilog.

In combination with [structured-log-seq-sink](https://github.com/Wedvich/structured-log-seq-sink) it can be used to write to Seq

To use this approach:


#### Include the libraries

Install both [structured-log npm](https://www.npmjs.com/package/structured-log) and [structured-log-seq-sink npm](https://www.npmjs.com/package/structured-log-seq-sink). Or include them from [jsDelivr](https://www.jsdelivr.com/):

<!-- snippet: StructuredLogInclude -->
<a id='cf26a040'></a>
```html
<script src='https://cdn.jsdelivr.net/npm/structured-log/dist/structured-log.js'>
</script>
<script src='https://cdn.jsdelivr.net/npm/structured-log-seq-sink/dist/structured-log-seq-sink.js'>
</script>
```
<sup><a href='/src/SampleWeb/sample.html#L4-L9' title='Snippet source file'>snippet source</a> | <a href='#cf26a040' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Configure the log

<!-- snippet: StructuredLogConfig -->
<a id='4b9342d5'></a>
```js
var levelSwitch = new structuredLog.DynamicLevelSwitch('info');
const log = structuredLog.configure()
    .writeTo(new structuredLog.ConsoleSink())
    .minLevel(levelSwitch)
    .writeTo(SeqSink({
        url: `${location.protocol}//${location.host}`,
        compact: true,
        levelSwitch: levelSwitch
    }))
    .create();
```
<sup><a href='/src/SampleWeb/sample.js#L1-L12' title='Snippet source file'>snippet source</a> | <a href='#4b9342d5' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Write a log message

<!-- snippet: StructuredLog -->
<a id='22034f8b'></a>
```js
function LogStructured(text) {
    log.info('StructuredLog input: {Text}', text);
}
```
<sup><a href='/src/SampleWeb/sample.js#L50-L54' title='Snippet source file'>snippet source</a> | <a href='#22034f8b' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Including data but omitting from the message template

When using structured-log, data not included in the message template will be named with a convention of `a+counter`. So for example if the following is logged:

```
log.info('The text: {Text}', text, "OtherData");
```

Then `OtherData` would be written to Seq with the property name `a1`.

To work around this:

Include a filter that replaces a known token name (in this case `{@Properties}`):

<!-- snippet: StructuredLogConfigExtraProp -->
<a id='a82a217c'></a>
```js
const logWithExtraProps = structuredLog.configure()
    .filter(logEvent => {
        const template = logEvent.messageTemplate;
        template.raw = template.raw.replace('{@Properties}','');
        return true;
    })
    .writeTo(SeqSink({
        url: `${location.protocol}//${location.host}`,
        compact: true,
        levelSwitch: levelSwitch
    }))
    .create();
```
<sup><a href='/src/SampleWeb/sample.js#L14-L27' title='Snippet source file'>snippet source</a> | <a href='#a82a217c' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Include that token name in the message template, and then include an object at the same position in the log parameters:

<!-- snippet: StructuredLogWithExtraProps -->
<a id='c29654aa'></a>
```js
function LogStructuredWithExtraProps(text) {
    logWithExtraProps.info(
        'StructuredLog input: {Text} {@Properties}',
        text,
        {
            Timezone: new Date().getTimezoneOffset(),
            Language: navigator.language
        });
}
```
<sup><a href='/src/SampleWeb/sample.js#L38-L48' title='Snippet source file'>snippet source</a> | <a href='#c29654aa' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then a destructured property will be written to Seq.

<img src="/src/omitFromMessage.png">


## Security contact information

To report a security vulnerability, use the [Tidelift security contact](https://tidelift.com/security). Tidelift will coordinate the fix and disclosure.


## Icon

[Robot](https://thenounproject.com/term/robot/883226/) designed by [Maxim Kulikov](https://thenounproject.com/maxim221) from [The Noun Project](https://thenounproject.com).
