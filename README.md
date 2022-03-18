What is it?
===========
A simple event sourcing "framework" based on Greg Young's work in DDD, CQRS and Event Sourcing. In truth, as you'll see, it's barely a "framework" as much of a bunch of reusable code. :)

Why'd you call it "Inforigami.Regalo"?
===========================
Well it's an event sourcing framework, and events tell a story. You might "regale" someone with a story, and I just swopped the trailing "e" for an "o" to make it sound cool. Hence "Inforigami.Regalo". I pronounce it "regarlo", in case you're wondering.

How do I use it?
================
Inforigami.Regalo comes in two significant parts - there's _Regalo.Core_ itself. This provides a bunch of interfaces and a Greg Young-inspired `AggregateRoot` class to derive your... aggregate roots from. You also get an event sourcing repository implementation. What's missing from that picture is the actual persistence. That's where _Inforigami.Regalo.RavenDB_ (and at some point _Inforigami.Regalo.EventStore_ and _Inforigami.Regalo.SqlServer_) come in. They provide an event store implementation.

Getting started is a case of installing one of the event store implementation packages via nuget.org e.g. `install-package Inforigami.Regalo.Ravendb`, then configuring a few dependencies (all Inforigami.Regalo libraries rely on the Dependency Inversion principle).

I'll try to build a "getting started" page on the wiki asap.

Full Disclosure
===============
There are a couple of things you probably won't like...

1. Having a reference to one of these assemblies in your Domain Model projects. There's really no problem here. You've made a decision to use this library, and you're not going to just swap it out for another one - you'd have to re-write everything anyway. It's opinionated, if you go with it you'll be fine.
2. Having a reference in your Domain Commands/Events. This is also fine. It seems like it would mean that you'd have to reference Inforigami.Regalo.Interfaces in consuming projects to, which would be bad. In fact, don't do that. Instead create proper inter-service events and an anti-corruption layer instead. Your domain events then stay inside your domain and your externally-published events are free to be happy little POCOs, just as intended.