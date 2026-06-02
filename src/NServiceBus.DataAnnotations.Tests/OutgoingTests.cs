public class OutgoingTests
{
    [Test]
    public Task With_no_validator()
    {
        var message = new MessageWithNoValidator();
        return Send(message);
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
    public async Task With_validator_invalid()
    {
        var message = new MessageWithValidator();
        var exception = await Assert.ThrowsAsync<MessageValidationException>(() => Send(message));
        await Verify(exception).IgnoreStackTrace();
    }

    static async Task Send(object message, [CallerMemberName] string key = "")
    {
        var builder = Host.CreateApplicationBuilder();
        var resetEvent = new ManualResetEvent(false);
        builder.Services.AddSingleton(resetEvent);
        var configuration = new EndpointConfiguration("DataAnnotationsOutgoing" + key);
        configuration.UseTransport<LearningTransport>();
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<SystemJsonSerializer>();

        configuration.UseDataAnnotationsValidation(incoming: false);

        builder.Services.AddNServiceBusEndpoint(configuration);
        using var host = builder.Build();
        await host.StartAsync();
        var session = host.Services.GetRequiredService<IMessageSession>();
        try
        {
            await session.SendLocal(message);
        }
        finally
        {
            await host.StopAsync();
        }
    }
}
