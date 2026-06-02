public class IncomingTests
{
    [Test]
    public async Task With_no_validator()
    {
        var message = new MessageWithNoValidator();
        await Assert.That(await Send(message)).IsNull();
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
        await Verify(await Send(message)).IgnoreStackTrace();
    }

    static async Task<MessageValidationException> Send(object message, [CallerMemberName] string key = "")
    {
        var builder = Host.CreateApplicationBuilder();

        var configuration = new EndpointConfiguration("DataAnnotationsIncoming" + key);
        configuration.UseTransport<LearningTransport>();
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<SystemJsonSerializer>();

        using var resetEvent = new ManualResetEvent(false);
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
        configuration.UseDataAnnotationsValidation(outgoing: false);

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
