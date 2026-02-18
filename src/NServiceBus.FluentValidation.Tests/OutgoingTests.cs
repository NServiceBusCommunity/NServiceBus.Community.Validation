using System.Threading.Tasks;

public class OutgoingTests
{
    [Test]
    public Task With_no_validator()
    {
        var message = new MessageWithNoValidator();
        return Send(message);
    }

    [Test]
    public Task With_no_validator_Fallback()
    {
        var message = new MessageWithNoValidator();
        return ThrowsTask(() => Send(message, fallback: _ => new FallbackValidator()));
    }

    class FallbackValidator : AbstractValidator<MessageWithNoValidator>
    {
        public FallbackValidator() =>
            RuleFor(_ => _.Content)
                .NotEmpty();
    }

    [Test]
    public Task With_validator_valid()
    {
        var message = new MessageWithValidator
        {
            Content = "content"
        };
        return Send(message);
    }

    [Test]
    public Task With_validator_invalid()
    {
        var message = new MessageWithValidator();
        return ThrowsTask(() => Send(message));
    }

    [Test]
    public Task With_async_validator_valid()
    {
        var message = new MessageWithAsyncValidator
        {
            Content = "content"
        };
        return Send(message);
    }

    [Test]
    public Task With_async_validator_invalid()
    {
        var message = new MessageWithAsyncValidator();
        return ThrowsTask(() => Send(message))
            .IgnoreStackTrace();
    }

    static async Task Send(
        object message,
        [CallerMemberName] string key = "",
        Func<Type, IValidator>? fallback = null)
    {
        var services = new ServiceCollection();
        var configuration = new EndpointConfiguration("FluentValidationOutgoing" + key);
        configuration.UseTransport<LearningTransport>();
        configuration.PurgeOnStartup(true);
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();

        configuration.UseFluentValidation(incoming: false, fallback: fallback);
        services.AddValidatorsFromAssemblyContaining<MessageWithNoValidator>(throwForNonPublicValidators: false);

        var endpointProvider = EndpointWithExternallyManagedContainer
            .Create(configuration, services);

        await using var provider = services.BuildServiceProvider();
        var endpoint = await endpointProvider.Start(provider);
        try
        {
            await endpoint.SendLocal(message);
        }
        finally
        {
            await endpoint.Stop();
        }
    }
}