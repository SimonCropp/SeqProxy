# <img src="https://raw.githubusercontent.com/SimonCropp/SeqProxy/master/src/icon.png" height="30px"> SeqProxy

Enables writing seq logs by poxying requests through an ASP.NET MVC Controller.


## Why

Avoid exposing the Seq API to the internet.

Leverage [Asp AUthentication and Authorization](https://docs.microsoft.com/en-us/aspnet/core/security/) to verify and control incoming requests.


## The NuGet package [![NuGet Status](http://img.shields.io/nuget/v/SeqProxy.svg)](https://www.nuget.org/packages/SeqProxy/)

https://nuget.org/packages/SeqProxy/

    PM> Install-Package SeqProxy


## HTTP Format/Protocol

Format: [Serilog compact](https://github.com/serilog/serilog-formatting-compact).

Protocol: [Seq raw events](https://docs.datalust.co/docs/posting-raw-events).


## Usage


### Enable in Startup

Enable in `Startup.ConfigureServices`

snippet: ConfigureServices


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


## Icon

<a href="http://thenounproject.com/term/robot/883226/">Robot</a> designed by <a href="https://thenounproject.com/maxim221/">Maxim Kulikov</a> from The Noun Project