# <img src="/src/icon.png" height="30px"> SeqProxy

[![Build status](https://ci.appveyor.com/api/projects/status/7996jd4uoooy5qy2/branch/main?svg=true)](https://ci.appveyor.com/project/SimonCropp/SeqProxy)
[![NuGet Status](https://img.shields.io/nuget/v/SeqProxy.svg)](https://www.nuget.org/packages/SeqProxy/)

Enables writing [Seq](https://datalust.co/seq) logs by proxying requests through an ASP.NET Controller or Middleware.

**See [Milestones](../../milestones?state=closed) for release notes.**


## Why

 * Avoid exposing the Seq API to the internet.
 * Leverage [Asp Authentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.
 * Append [extra data](#extra-data) to log messages during server processing.


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
<a id='snippet-BuildId'></a>
```cs
var startOfYear = new DateTime(utcNow.Year, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
var ticks = utcNow.Ticks - startOfYear.Ticks;
var id = ticks.ToString("x");
```
<sup><a href='/src/SeqProxy/SeqWriter.cs#L94-L100' title='Snippet source file'>snippet source</a> | <a href='#snippet-BuildId' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Which generates a string of the form `8e434f861302`. The current year is trimmed to shorten the id and under the assumption that retention policy is not longer than 12 months. There is a small chance of collisions, but given the use-case (error correlation), this should not impact the ability to find the correct error. This string can then be given to a user as a error correlation id.

Then the log entry can be accessed using a Seq filter.

`http://seqServer/#/events?filter=SeqProxyId%3D'39f616eeb2e3'`


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

<!-- snippet: ConfigureServices -->
<a id='snippet-ConfigureServices'></a>
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore(option => option.EnableEndpointRouting = false);
    services.AddSeqWriter(seqUrl: "http://localhost:5341");
}
```
<sup><a href='/src/SampleWeb/Startup.cs#L5-L13' title='Snippet source file'>snippet source</a> | <a href='#snippet-ConfigureServices' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

There are several optional parameters:

<!-- snippet: ConfigureServicesFull -->
<a id='snippet-ConfigureServicesFull'></a>
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddMvcCore();
    services.AddSeqWriter(
        seqUrl: "http://localhost:5341",
        apiKey: "TheApiKey",
        application: "MyAppName",
        appVersion: new(1, 2),
        scrubClaimType: claimType =>
        {
            var lastIndexOf = claimType.LastIndexOf('/');
            if (lastIndexOf == -1)
            {
                return claimType;
            }

            return claimType[(lastIndexOf + 1)..];
        });
}
```
<sup><a href='/src/Tests/FullStartupConfig.cs#L4-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-ConfigureServicesFull' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

 * `application` defaults to `Assembly.GetCallingAssembly().GetName().Name`.
 * `applicationVersion` defaults to `Assembly.GetCallingAssembly().GetName().Version`.
 * `scrubClaimType` is used to clean up claimtype strings. For example [ClaimTypes.Email](https://docs.microsoft.com/en-us/dotnet/api/system.identitymodel.claims.claimtypes.email?System_IdentityModel_Claims_ClaimTypes_Email) is `http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress`, but when recording to Seq the value `emailaddress` is sufficient. Defaults to `DefaultClaimTypeScrubber.Scrub` to get the string after the last `/`.

<!-- snippet: DefaultClaimTypeScrubber.cs -->
<a id='snippet-DefaultClaimTypeScrubber.cs'></a>
```cs
namespace SeqProxy;

/// <summary>
/// Used for scrubbing claims when no other scrubber is defined.
/// </summary>
public static class DefaultClaimTypeScrubber
{
    /// <summary>
    /// Get the string after the last /.
    /// </summary>
    public static CharSpan Scrub(CharSpan claimType)
    {
        Guard.AgainstEmpty(claimType, nameof(claimType));
        var lastIndexOf = claimType.LastIndexOf('/');
        if (lastIndexOf == -1)
        {
            return claimType;
        }

        return claimType[(lastIndexOf + 1)..];
    }
}
```
<sup><a href='/src/SeqProxy/DefaultClaimTypeScrubber.cs#L1-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-DefaultClaimTypeScrubber.cs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Add HTTP handling

There are two approaches to handling the HTTP containing log events. Using a Middleware and using a Controller.


#### Using a Middleware

Using a [Middleware](https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/) is done by calling `SeqWriterConfig.UseSeq` in `Startup.Configure(IApplicationBuilder builder)`:

<!-- snippet: ConfigureBuilder -->
<a id='snippet-ConfigureBuilder'></a>
```cs
public void Configure(IApplicationBuilder builder)
{
    builder.UseSeq();
```
<sup><a href='/src/SampleWeb/Startup.cs#L15-L20' title='Snippet source file'>snippet source</a> | <a href='#snippet-ConfigureBuilder' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Authorization

Authorization in the middleware can bu done by using `useAuthorizationService = true` in `UseSeq`.

<!-- snippet: StartupWithAuth -->
<a id='snippet-StartupWithAuth'></a>
```cs
public void Configure(IApplicationBuilder builder)
{
    builder.UseSeq(useAuthorizationService: true);
```
<sup><a href='/src/Tests/StartupWithAuth.cs#L5-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-StartupWithAuth' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This then uses [IAuthorizationService](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/resourcebased) to verify the request:

<!-- snippet: HandleWithAuth -->
<a id='snippet-HandleWithAuth'></a>
```cs
async Task HandleWithAuth(HttpContext context)
{
    var user = context.User;
    var authResult = await authService.AuthorizeAsync(user, null, "SeqLog");

    if (!authResult.Succeeded)
    {
        await context.ChallengeAsync();
        return;
    }

    await writer.Handle(
        user,
        context.Request,
        context.Response,
        context.RequestAborted);
}
```
<sup><a href='/src/SeqProxy/SeqMiddlewareWithAuth.cs#L15-L35' title='Snippet source file'>snippet source</a> | <a href='#snippet-HandleWithAuth' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Using a Controller

`BaseSeqController` is an implementation of `ControllerBase` that provides a HTTP post and some basic routing.

<!-- snippet: BaseSeqController.cs -->
<a id='snippet-BaseSeqController.cs'></a>
```cs
namespace SeqProxy;

/// <summary>
/// An implementation of <see cref="ControllerBase"/> that provides a http post and some basic routing.
/// </summary>
[Route("/api/events/raw")]
[Route("/seq")]
[ApiController]
public abstract class BaseSeqController :
    ControllerBase
{
    SeqWriter writer;

    /// <summary>
    /// Initializes a new instance of <see cref="BaseSeqController"/>
    /// </summary>
    protected BaseSeqController(SeqWriter writer) =>
        this.writer = writer;

    /// <summary>
    /// Handles log events via a HTTP post.
    /// </summary>
    [HttpPost]
    public virtual Task Post() =>
        writer.Handle(User, Request, Response, HttpContext.RequestAborted);
}
```
<sup><a href='/src/SeqProxy/BaseSeqController.cs#L1-L26' title='Snippet source file'>snippet source</a> | <a href='#snippet-BaseSeqController.cs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Add a new [controller](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) that overrides `BaseSeqController`.

<!-- snippet: SimpleController -->
<a id='snippet-SimpleController'></a>
```cs
public class SeqController(SeqWriter writer) :
    BaseSeqController(writer);
```
<sup><a href='/src/Tests/ControllerSamples.cs#L3-L6' title='Snippet source file'>snippet source</a> | <a href='#snippet-SimpleController' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Authorization/Authentication

Adding authorization and authentication can be done with an [AuthorizeAttribute](https://docs.microsoft.com/en-us/aspnet/core/security/authorization/simple).

<!-- snippet: AuthorizeController -->
<a id='snippet-AuthorizeController'></a>
```cs
[Authorize]
public class SeqController(SeqWriter writer) :
    BaseSeqController(writer)
```
<sup><a href='/src/Tests/ControllerSamples.cs#L28-L33' title='Snippet source file'>snippet source</a> | <a href='#snippet-AuthorizeController' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


##### Method level attributes

Method level Asp attributes can by applied by overriding `BaseSeqController.Post`.

For example adding an [exception filter ](https://docs.microsoft.com/en-us/aspnet/core/mvc/controllers/filters#exception-filters).

<!-- snippet: OverridePostController -->
<a id='snippet-OverridePostController'></a>
```cs
public class SeqController(SeqWriter writer) :
    BaseSeqController(writer)
{
    [CustomExceptionFilter]
    public override Task Post() =>
        base.Post();
```
<sup><a href='/src/Tests/ControllerSamples.cs#L11-L19' title='Snippet source file'>snippet source</a> | <a href='#snippet-OverridePostController' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Client Side Usage


### Using raw JavaScript

Writing to Seq can be done using a HTTP post:

<!-- snippet: LogRawJs -->
<a id='snippet-LogRawJs'></a>
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
<sup><a href='/src/SampleWeb/sample.js#L59-L69' title='Snippet source file'>snippet source</a> | <a href='#snippet-LogRawJs' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Using Structured-Log

[structured-log](https://github.com/structured-log/structured-log/) is a structured logging framework for JavaScript, inspired by Serilog.

In combination with [structured-log-seq-sink](https://github.com/Wedvich/structured-log-seq-sink) it can be used to write to Seq

To use this approach:


#### Include the libraries

Install both [structured-log npm](https://www.npmjs.com/package/structured-log) and [structured-log-seq-sink npm](https://www.npmjs.com/package/structured-log-seq-sink). Or include them from [jsDelivr](https://www.jsdelivr.com/):

<!-- snippet: StructuredLogInclude -->
<a id='snippet-StructuredLogInclude'></a>
```html
<script src='https://cdn.jsdelivr.net/npm/structured-log/dist/structured-log.js'>
</script>
<script src='https://cdn.jsdelivr.net/npm/structured-log-seq-sink/dist/structured-log-seq-sink.js'>
</script>
```
<sup><a href='/src/SampleWeb/sample.html#L4-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-StructuredLogInclude' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Configure the log

<!-- snippet: StructuredLogConfig -->
<a id='snippet-StructuredLogConfig'></a>
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
<sup><a href='/src/SampleWeb/sample.js#L1-L12' title='Snippet source file'>snippet source</a> | <a href='#snippet-StructuredLogConfig' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


#### Write a log message

<!-- snippet: StructuredLog -->
<a id='snippet-StructuredLog'></a>
```js
function LogStructured(text) {
    log.info('StructuredLog input: {Text}', text);
}
```
<sup><a href='/src/SampleWeb/sample.js#L50-L54' title='Snippet source file'>snippet source</a> | <a href='#snippet-StructuredLog' title='Start of snippet'>anchor</a></sup>
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
<a id='snippet-StructuredLogConfigExtraProp'></a>
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
<sup><a href='/src/SampleWeb/sample.js#L14-L27' title='Snippet source file'>snippet source</a> | <a href='#snippet-StructuredLogConfigExtraProp' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Include that token name in the message template, and then include an object at the same position in the log parameters:

<!-- snippet: StructuredLogWithExtraProps -->
<a id='snippet-StructuredLogWithExtraProps'></a>
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
<sup><a href='/src/SampleWeb/sample.js#L38-L48' title='Snippet source file'>snippet source</a> | <a href='#snippet-StructuredLogWithExtraProps' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Then a destructured property will be written to Seq.

<img src="/src/omitFromMessage.png">


## Icon

[Robot](https://thenounproject.com/term/robot/883226/) designed by [Maxim Kulikov](https://thenounproject.com/maxim221) from [The Noun Project](https://thenounproject.com).
