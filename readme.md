# <img src="/src/icon.png" height="25px"> NServiceBus.Community.Validation

[![Build status](https://img.shields.io/appveyor/build/SimonCropp/nservicebus-community-validation)](https://ci.appveyor.com/project/SimonCropp/nservicebus-community-validation)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.Community.DataAnnotations.svg?label=NServiceBus.Community.DataAnnotations)](https://www.nuget.org/packages/NServiceBus.Community.DataAnnotations/)
[![NuGet Status](https://img.shields.io/nuget/v/NServiceBus.Community.FluentValidation.svg?label=NServiceBus.Community.FluentValidation)](https://www.nuget.org/packages/NServiceBus.Community.FluentValidation/)

Message validation using [DataAnnotations](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.aspx) and [FluentValidation](https://github.com/JeremySkinner/FluentValidation).

**See [Milestones](../../milestones?state=closed) for release notes.**

<!--- StartOpenCollectiveBackers -->

[Already a Patron? skip past this section](#endofbacking)


## Community backed

**It is expected that all developers [become a Patron](https://opencollective.com/nservicebuscommunity/contribute/patron-6976) to use NServiceBus Community Extensions. [Go to licensing FAQ](https://github.com/NServiceBusCommunity/Home/#licensingpatron-faq)**


### Sponsors

Support this project by [becoming a Sponsor](https://opencollective.com/nservicebuscommunity/contribute/sponsor-6972). The company avatar will show up here with a website link. The avatar will also be added to all GitHub repositories under the [NServiceBusCommunity organization](https://github.com/NServiceBusCommunity).


### Patrons

Thanks to all the backing developers. Support this project by [becoming a patron](https://opencollective.com/nservicebuscommunity/contribute/patron-6976).

<img src="https://opencollective.com/nservicebuscommunity/tiers/patron.svg?width=890&avatarHeight=60&button=false">

<a href="#" id="endofbacking"></a>

<!--- EndOpenCollectiveBackers -->


## NServiceBus.FluentValidation

Uses [FluentValidation](https://github.com/JeremySkinner/FluentValidation) to validate incoming and outgoing messages.


### NuGet package

https://www.nuget.org/packages/NServiceBus.Community.FluentValidation/


### Usage

FluentValidation message validation can be enabled using the following:

<!-- snippet: FluentValidation -->
<a id='snippet-FluentValidation'></a>
```cs
endpointConfiguration.UseFluentValidation();
serviceCollection.AddValidatorsFromAssemblyContaining<TheMessage>();
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/Usage.cs#L5-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This will result in, when an invalid message being detected, a validation exception being thrown and that message being handled by [Recoverability](/nservicebus/recoverability/). The validation exception will also be added to [Unrecoverable exceptions](/nservicebus/recoverability/#unrecoverable-exceptions) to avoid unnecessary retries.<!-- singleLineInclude: validationexception. path: /src/validationexception.include.md -->

By default, incoming and outgoing messages are validated.

To disable for incoming messages use the following:

<!-- snippet: FluentValidation_disableincoming -->
<a id='snippet-FluentValidation_disableincoming'></a>
```cs
endpointConfiguration.UseFluentValidation(incoming: false);
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/Usage.cs#L12-L16' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_disableincoming' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To disable for outgoing messages use the following:

<!-- snippet: FluentValidation_disableoutgoing -->
<a id='snippet-FluentValidation_disableoutgoing'></a>
```cs
endpointConfiguration.UseFluentValidation(outgoing: false);
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/Usage.cs#L18-L22' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_disableoutgoing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Enabling validation on outgoing message will result in the validation exception be thrown in the context of the sender, instead of during message processing on the receiving endpoint. This can be particularly helpful in development and/or debugging scenarios since the stack trace and debugger will more accurately reflect the cause of the invalid message.<!-- singleLineInclude: validationoutgoing. path: /src/validationoutgoing.include.md -->

Messages can then have an associated [validator](https://github.com/JeremySkinner/FluentValidation/wiki/b.-Creating-a-Validator):

<!-- snippet: FluentValidation_message -->
<a id='snippet-FluentValidation_message'></a>
```cs
public class TheMessage :
    IMessage
{
    public string Content { get; set; } = null!;
}

public class MyMessageValidator :
    AbstractValidator<TheMessage>
{
    public MyMessageValidator() =>
        RuleFor(_ => _.Content)
            .NotEmpty();
}
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/TheMessage.cs#L1-L17' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_message' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Accessing the current pipeline context

In some cases a validator may need to use data from the current message context.

The current message context can be accessed via two extension methods:

 * The current [message headers](https://docs.particular.net/nservicebus/messaging/headers) can be accessed via `FluentValidationExtensions.Headers(this CustomContext customContext)`
 * The current `ContextBag` can be accessed via `FluentValidationExtensions.ContextBag(this CustomContext customContext)`.

<!-- snippet: FluentValidation_ContextValidator -->
<a id='snippet-FluentValidation_ContextValidator'></a>
```cs
public class ContextValidator :
    AbstractValidator<TheMessage>
{
    public ContextValidator() =>
        RuleFor(_ => _.Content)
            .Custom((propertyValue, validationContext) =>
            {
                var messageHeaders = validationContext.Headers();
                var bag = validationContext.ContextBag();
                if (propertyValue != "User" ||
                    messageHeaders.ContainsKey("Auth"))
                {
                    return;
                }

                validationContext.AddFailure("Expected Auth header to exist");
            });
}
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/ContextValidator.cs#L3-L24' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_ContextValidator' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


### Validator scanning

Validators are registered and resolved using [dependency injection](https://docs.particular.net/nservicebus/dependency-injection/). Assemblies can be added for validator scanning using either a generic Type, a Type instance, or an assembly instance.

<!-- snippet: FluentValidation_AddValidators -->
<a id='snippet-FluentValidation_AddValidators'></a>
```cs
endpointConfiguration.UseFluentValidation();
serviceCollection.AddValidatorsFromAssemblyContaining<MyMessage>();
serviceCollection.AddValidatorsFromAssemblyContaining(typeof(SomeOtherMessage));
serviceCollection.AddValidatorsFromAssembly(assembly);
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/Usage.cs#L27-L34' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_AddValidators' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


By default, there are two exception scenarios when adding validators. An exception will be thrown if:

 * No validators are found in an assembly that is scanned.
 * Any non-public validators are found in an assembly that is scanned.

These exception scenarios can be excluded using the following:

<!-- snippet: FluentValidation_IgnoreValidatorConventions -->
<a id='snippet-FluentValidation_IgnoreValidatorConventions'></a>
```cs
endpointConfiguration.UseFluentValidation();
serviceCollection.AddValidatorsFromAssembly(
    assembly,
    throwForNonPublicValidators: false,
    throwForNoValidatorsFound: false);
```
<sup><a href='/src/NServiceBus.FluentValidation.Tests/Snippets/Usage.cs#L39-L47' title='Snippet source file'>snippet source</a> | <a href='#snippet-FluentValidation_IgnoreValidatorConventions' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## NServiceBus.DataAnnotations

Uses [System.ComponentModel.DataAnnotations](https://msdn.microsoft.com/en-us/library/cc490428) to validate incoming and outgoing messages.


### NuGet package

https://www.nuget.org/packages/NServiceBus.DataAnnotations/


### Usage


DataAnnotations message validation can be enabled using the following:

<!-- snippet: DataAnnotations -->
<a id='snippet-DataAnnotations'></a>
```cs
configuration.UseDataAnnotationsValidation();
```
<sup><a href='/src/NServiceBus.DataAnnotations.Tests/Snippets/Usage.cs#L5-L9' title='Snippet source file'>snippet source</a> | <a href='#snippet-DataAnnotations' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

This will result in, when an invalid message being detected, a validation exception being thrown and that message being handled by [Recoverability](/nservicebus/recoverability/). The validation exception will also be added to [Unrecoverable exceptions](/nservicebus/recoverability/#unrecoverable-exceptions) to avoid unnecessary retries.<!-- singleLineInclude: validationexception. path: /src/validationexception.include.md -->

By default, incoming and outgoing messages are validated.

To disable for incoming messages use the following:

<!-- snippet: DataAnnotations_disableincoming -->
<a id='snippet-DataAnnotations_disableincoming'></a>
```cs
configuration.UseDataAnnotationsValidation(incoming: false);
```
<sup><a href='/src/NServiceBus.DataAnnotations.Tests/Snippets/Usage.cs#L11-L15' title='Snippet source file'>snippet source</a> | <a href='#snippet-DataAnnotations_disableincoming' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

To disable for outgoing messages use the following:

<!-- snippet: DataAnnotations_disableoutgoing -->
<a id='snippet-DataAnnotations_disableoutgoing'></a>
```cs
configuration.UseDataAnnotationsValidation(outgoing: false);
```
<sup><a href='/src/NServiceBus.DataAnnotations.Tests/Snippets/Usage.cs#L17-L21' title='Snippet source file'>snippet source</a> | <a href='#snippet-DataAnnotations_disableoutgoing' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->

Enabling validation on outgoing message will result in the validation exception be thrown in the context of the sender, instead of during message processing on the receiving endpoint. This can be particularly helpful in development and/or debugging scenarios since the stack trace and debugger will more accurately reflect the cause of the invalid message.<!-- singleLineInclude: validationoutgoing. path: /src/validationoutgoing.include.md -->

Messages can then be decorated with DataAnnotations attributes. For example, to make a property required use the [RequiredAttribute](https://msdn.microsoft.com/en-us/library/system.componentmodel.dataannotations.requiredattribute.aspx):

<!-- snippet: DataAnnotations_message -->
<a id='snippet-DataAnnotations_message'></a>
```cs
public class TheMessage :
    IMessage
{
    [Required]
    public string Content { get; set; } = null!;
}
```
<sup><a href='/src/NServiceBus.DataAnnotations.Tests/Snippets/TheMessage.cs#L1-L10' title='Snippet source file'>snippet source</a> | <a href='#snippet-DataAnnotations_message' title='Start of snippet'>anchor</a></sup>
<!-- endSnippet -->


## Icon

[Validation](https://thenounproject.com/term/validation/1680887/) designed by [Becris](https://thenounproject.com/Becris/) from [The Noun Project](https://thenounproject.com/).
