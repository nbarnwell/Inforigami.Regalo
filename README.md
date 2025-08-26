# What is it?

A few libraries that can be combined together to make build systems based on DDD, CQRS, and EventSourcing patterns. There is support for SQL Server, RavenDB, EventStoreDB, and Azure Table Storage (experimental) for your event streams. Credit for the design that became the `AggregateRoot` implementation has to go to [Greg Young](https://x.com/gregyoung).

_"Why'd you call it "Inforigami.Regalo"?"_
Well it's an event sourcing framework, and events tell a story. You might "regale" someone with a story, and I just swopped the trailing "e" for an "o". Hence "Inforigami.Regalo". I pronounce it "regarlo", in case you're wondering.

# How do I get it?

All projects that you would use to implement your application are available on nuget.org. See [Getting Started](#getting-started) for more information.

# Patterns

There are a number of related patterns that are so closely related that they are often used interchangably to mean the same thing:

The basic principles of trying to keep your raw "business logic"/"domain logic" code away from "infrastructure" concerns like HTTP/REST, queuing, databases, email sending, etc are valid goals, but the terminology and understanding are not always consistent.

The following is brief explanation of how Regalo implements each of these related patterns:

| Regalo | Domain-Driven Design | Hexagonal/Ports-and-Adapters Architecture | Clean Architecture |
|--------|----------------------|------------------------|--------------------|
| `Inforigami.Regalo.EventSourcing.AggregateRoot` | Domain Model, Aggregate Root, Entity | Business Logic | Entities |
| `Inforigami.Regalo.Interfaces.ICommand/Command` | Domain Model, Command | Business Logic | Entities |
| `Inforigami.Regalo.Interfaces.IEvent/Event` | Domain Model, Event | Business Logic | Entities |
| `Inforigami.Regalo.Messaging.ICommandHandler<T>/IEventHandler<T>` | Domain Model, Application Service | Port | Use-Cases |
| Framework e.g. ASP.NET Controller, NServiceBus Handler | Infrastructure Layer | Adapter | Presenters, Gateways, Controllers |


# Getting Started

## Creating a domain model project

**Note:**  
I recommend you keep related aggregates and their domain Commands, Events, and Application Services together in namespaces.

1. Choose from the available persistence options for your event streams below:
    * [SQL Server](#inforigamiregalosqlserver)
    * [EventStoreDB](#inforigamiregaloeventstore)
    * [RavenDB](#inforigamiregaloravendb)
    * [Azure Table Storage](#inforigamiregaloazuretablestorage) (experimental)
1. Create a new class library project for your domain model and delete the scaffolded `Class1.cs`.
1. Install your chosen persistence package.
1. Choose your first Command to implement. This will be informed by the tasks to be carried out by your use-cases.
1. Refer to the guidance for [Creating a domain command](#creating-a-domain-command).
1. Refer to the guidance for [Creating a domain event](#creating-a-domain-event).
1. Refer to the guidance for [Building an Aggregate Root](#building-an-aggregate-root).
1. Refer to the guidance for [Creating an Application Service](#creating-an-application-service).
1. If your domain events need to leave your system and be consumed by another system (e.g. in a microservices environment) then I recommend creating a new class library project that contains POCOs for those events, and build some mapping to them from your "true" domain events. This means consumers of your events don't have to take a dependency on your domain model project and therefore any other dependencies like Regalo that it would bring.

## Creating a domain command

Any given use-case may be made up of multiple tasks. Creating a shopping basket, adding items to it, removing items, adding voucher codes, etc are all tasks. Think of Commands as representing the various tasks. You'll have a Command, a Command Handler (see guidance for [Creating an Application Service](#creating-an-application-service)), a method on an AggregateRoot, one or more Events that can be generated as a result of invoking that aggregate's behaviour, and zero or more Event Handlers (also Application Services) that may do something with those events.

1. Create a class representing a Command that inherits `Inforigami.Regalo.Interfaces.Command`.
1. Create a `public Guid Id { get; private set; }` property to store the aggregate's ID. This step is optional for Commands that create aggregates.
1. Create a `public int Version { get; private set; }` property to store the version of the aggregate that the command applies to. This step is optional for Commands that create aggregates.
1. Create similar properties for any other values that apply to the command.
1. Create a constructor that initialises all the properties.

## Creating a domain event

1. Create a class representing an Event that inherits `Inforigami.Regalo.Interfaces.Event`.
1. Create a `public Guid Id { get; private set; }` property to store the aggregate's ID.
1. Create a `public int Version { get; private set; }` property to store the version of the aggregate that the command applies to.
1. Create similar properties for any other values that apply to the command.
1. Create a constructor that initialises all the properties.

## Building an Aggregate Root

* Aggregate Roots may implement business rules, invariants and such. They MUST NOT use infrastructure (e.g. databases, email servers, send messages, etc). The command or event handler that loaded/created the aggregate root object and invoked it's behaviour is responsible for providing it everything it needs to do it's work.
* The public API for an aggregate root will consist entirely of `public void` methods that are named for Commands in your domain relating to that aggregate. There are to be no public properties or fields.
* Each public "command" method is only allowed to perform three duties:
    1. Validate parameters.
    1. Assert invariant logic (this is the actual "business logic" of your domain).
    1. Record new events by calling `base.Record()`.
* Public "command" methods should NOT modify private state of the aggregate.
* For each event that a "command" method may raise, there can be a corresponding `private void Apply([Event] evt)` method.
* Each private "apply event" method is only allowed to modify private state of the aggregate.
* Private "apply event" methods MUST NOT perform invariant logic, validation, or record further events.

#### To begin a new aggregate root

1. Create a class representing an aggregate root, ensuring it inherits `Inforigami.Regalo.EventSourcing.AggregateRoot`. E.g. `SalesOrder`.
1. Create an "initialisation" method that represents the aggregate's "initialisation" Command. E.g. `public void Create()`.
1. In that method, implement command validation, invariant logic, and `Record()` any events as necessary.  
    **Note**  
    Use exceptions to reject command execution.
1. Create the matching `private void Apply(AggregateCreated evt)` "apply event" method.  
    **Note**  
    In that method, ensure it sets `base.Id` from the Id on the event, plus any other private state that might be needed by other invariants in "command" methods on the same aggregate.

#### To add new command methods to an existing aggregate root

1. Create a "command" method on your aggregate
1. In that method, implement command validation, invariant logic, and `Record()` any events as necessary.  
    **Note**  
    Use exceptions to reject command execution.
1. Create the matching `private void Apply(AggregateCreated evt)` "apply event" methods, that will modify any private state that might be needed by other invariants in "command" methods on the same aggregate.

### Creating an Application Service

* In Regalo, an Application Service is an implementation of `Inforigami.Regalo.Messaging.ICommandHandler<T>` or `Inforigami.Regalo.Messaging.IEventHandler<T>`.
* Aggregate Roots may implement business rules, invariants and such. They MUST NOT use infrastructure (e.g. databases, email servers, send messages, etc). The command or event handler that loaded/created the aggregate root object and invoked it's behaviour is responsible for providing it everything it needs to do it's work, and MUST NOT implement any business rules.

#### To create an application service that invokes business logic

1. Create a class implementing `Inforigami.Regalo.Messaging.ICommandHandler<T>` or `Inforigami.Regalo.Messaging.IEventHandler<T>`.
    * **Tip**  
    Regalo's message-handling supports the entire message type hierarchy. So if you create `AllCommandsHandler<Command>` it will be invoked for any message that inherits `Command`. If you create type hierarchies for messages (e.g. for updating readstores or logging) you can make use of this feature easily.
1. Ensure your new handler has a `private readonly IMessageHandlerContext<TEntity> _context;` field initialised by the constructor.
1. In the `public void Handle<T>` method:
    1. Obtain a session from the context.
    1. Create a new AggregateRoot object or load it from the session using values on the message.
    1. Invoke a "command" method on the aggregate root object.
    1. Call `session.SaveAndPublishEvents()`.
    1. You should have something like the following:  
        ``` csharp
        public void Handle(PlaceSalesOrder command)
        {
            using (var session = _context.OpenSession(command))
            {
                var order = session.Get(command.SalesOrderId, command.SalesOrderVersion);
                order.PlaceOrder();
                session.SaveAndPublishEvents(order);
            }
        }
        ```

#### To create an application service that performs infrastructure tasks

This will be typically things like sending an email, publishing the message to a queuing service, etc and will most commonly be required for event handling.

1. Create a class implementing `Inforigami.Regalo.Messaging.ICommandHandler<T>` or `Inforigami.Regalo.Messaging.IEventHandler<T>`.
1. Implement the `public void Handle<T>` method as you see fit, using whatever injected services are required.


# What does each project do?

## Projects you consume in your applications

### Core projects

These are the basic projects that you are likely to require in any Regalo-based system. You may not need to install them directly as they will arrive as transient dependencies when you install other packages.

#### Inforigami.Regalo.Core  

As it's name suggests, a library of shared code that is used throughout the other libraries. Classes of note include:

* `DateTimeOffsetProvider`  
    Useful for writing testable code by making date generation deterministic by avoiding `DateTimeOffset.Now`.
* `GuidProvider`  
    Useful for writing testable code by making Guid generation deterministic by avoiding `Guid.NewGuid()`.
* `Resolver`  
    A basic Service Locator used when new objects are required after the composition root (e.g. WebAPI controller) has already been initialised. E.g. For initialising message handlers ( classes you provide implementing `Inforigami.Regalo.Messaging.IMessageHandler<TMessage>`). You will need to tell the `Resolver` how to use your chosen inversion of control container by calling `Resolver.Configure()` when your application is started.

#### Inforigami.Regalo.EventSourcing  

The key to this project is the `AggregateRoot` base class that you would use for all your aggregate root entities. Persistence-specific implementations are available and documented in the next section.

#### Inforigami.Regalo.Interfaces  

These are marker interfaces and base classes for domain events. For the events that leave your system you should have lightweight implementations, and appropriate mapping, so that external systems using your message contracts do not require even this lightweight dependency.

#### Inforigami.Regalo.Messaging  

A lightweight mediator pattern implementation. Command and Event handlers are the Application Services of your domain and will interact with your entities. Note that an ASP.NET Controller or NServiceBus/MassTransit handler would delegate to these classes if you choose to use this library. See notes above on how the various distributed systems patterns relate to each other and are implemented by Regalo.

#### Inforigami.Regalo.ObjectCompare  

You can use this library to perform a recursive property-based comparison of two objects. This is "dog-fooded" in the Inforigami.Regalo.Testing project.

#### Inforigami.Regalo.Testing  

A library you can use to make unit testing of your Application Services (handler implementations of Inforigami.Regalo.Messaging) and how they interact with your domain objects (implementations of `Inforigami.Regalo.EventSourcing.AggregateRoot`).

### Event stream persistence options

Depending on how you wish to store your event streams, you can choose one of the following:

#### Inforigami.Regalo.AzureTableStorage  
    An Azure Table Storage-based implementation of `Inforigami.Regalo.EventSourcing.IEventStore`.

#### Inforigami.Regalo.EventStore  
    An EventStoreDB-based implementation of `Inforigami.Regalo.EventSourcing.IEventStore`.

#### Inforigami.Regalo.RavenDB  
    A RavenDB-based implementation of `Inforigami.Regalo.EventSourcing.IEventStore`.

#### Inforigami.Regalo.SqlServer  
    A SQL Server-based implementation of `Inforigami.Regalo.EventSourcing.IEventStore`.

## Projects used only in the development of Regalo

#### Inforigami.Regalo.Core.ConsoleLoggerTest  
    A test-harness for the basic console logging provided by `Inforigami.Regalo.Core.ConsoleLogger`.

#### Inforigami.Regalo.Core.Tests.DomainModel  
    A sample domain model that is also used by Regalo's own tests.

# Principles

These libraries have certain opinions:

1. `AggregateRoot` follows the principle that aggregate roots do not have any public properties. They have only public methods, where each method should relate to part of a business transaction or use-case. The library will collect domain events that are generated when behaviours are invoked on 
1. You need a reference to one of these assemblies in your Domain Model projects. There's really no problem here. You've made a decision to use this library, and you're not going to just swap it out for another one - you'd have to re-write everything anyway. It's opinionated, if you go with it you'll be fine.
2. Having a reference in your Domain Commands/Events. This is also fine. It seems like it would mean that you'd have to reference Inforigami.Regalo.Interfaces in consuming projects to, which would be bad. In fact, don't do that. Instead create proper inter-service events and an anti-corruption layer instead. Your domain events then stay inside your domain and your externally-published events are free to be happy little POCOs, just as intended.