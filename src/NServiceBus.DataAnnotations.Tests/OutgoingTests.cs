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
        var services = new ServiceCollection();
        var resetEvent = new ManualResetEvent(false);
        services.AddSingleton(resetEvent);
        var configuration = new EndpointConfiguration("DataAnnotationsOutgoing" + key);
        configuration.UseTransport<LearningTransport>();
        configuration.PurgeOnStartup(true);
        configuration.UseSerialization<SystemJsonSerializer>();

        configuration.UseDataAnnotationsValidation(incoming: false);

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
