public class IncomingTests
{
    [Test]
    public async Task With_no_validator()
    {
        var message = new MessageWithNoValidator();
        await Assert.That(await Send(message)).IsNull();
    }

    [Test]
    public async Task With_no_validator_Fallback()
    {
        var message = new MessageWithNoValidator();
        await Assert.That(await Send(message, fallback: _ => new FallbackValidator())).IsNotNull();
    }

    class FallbackValidator : AbstractValidator<MessageWithNoValidator>
    {
        public FallbackValidator() =>
            RuleFor(_ => _.Content)
                .NotEmpty();
    }

    [Test]
    public async Task With_validator_valid()
    {
        var message = new MessageWithValidator
        {
            Content = "content"
        };
        await Assert.That(await Send(message)).IsNull();
    }

    [Test]
    public async Task With_validator_invalid()
    {
        var message = new MessageWithValidator();
        await Assert.That(await Send(message)).IsNotNull();
    }

    [Test]
    public async Task With_async_validator_valid()
    {
        var message = new MessageWithAsyncValidator
        {
            Content = "content"
        };
        await Assert.That(await Send(message)).IsNull();
    }

    [Test]
    public async Task With_async_validator_invalid()
    {
        var message = new MessageWithAsyncValidator();
        var exception = await Send(message);
        await Verify(exception);
    }

    [Test]
    public async Task Exception_message_and_errors()
    {
        var message = new MessageWithValidator();
        var exception = await Send(message);
        await Verify(exception);
    }

    static async Task<MessageValidationException> Send(
        object message,
        [CallerMemberName] string key = "",
        Func<Type, IValidator>? fallback = null)
    {
        var builder = Host.CreateApplicationBuilder();
        var configuration = new EndpointConfiguration("FluentValidationIncoming" + key);
        configuration.UseTransport<LearningTransport>();
        configuration.PurgeOnStartup(true);
        configuration.UsePersistence<LearningPersistence>();
        configuration.UseSerialization<SystemJsonSerializer>();

        var resetEvent = new ManualResetEvent(false);
        builder.Services.AddSingleton(resetEvent);
        MessageValidationException exception = null!;
        var recoverability = configuration.Recoverability();
        recoverability.CustomPolicy(
            (_, context) =>
            {
                exception = (MessageValidationException) context.Exception;
                resetEvent.Set();
                return RecoverabilityAction.Discard("error");
            });
        configuration.UseFluentValidation(outgoing: false, fallback: fallback);
        builder.Services.AddValidatorsFromAssemblyContaining<MessageWithNoValidator>(throwForNonPublicValidators: false);

        builder.Services.AddNServiceBusEndpoint(configuration);

        using var host = builder.Build();
        await host.StartAsync();
        var session = host.Services.GetRequiredService<IMessageSession>();
        await session.SendLocal(message);
        if (!resetEvent.WaitOne(TimeSpan.FromSeconds(10)))
        {
            if (exception == null)
            {
                throw new("No Set or exception received.");
            }
        }

        await host.StopAsync();
        return exception;
    }
}
