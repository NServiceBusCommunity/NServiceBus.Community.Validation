using NServiceBus.FluentValidation.Testing;
using VerifyTests.NServiceBus;
using System.Threading.Tasks;

public class HandlerTests
{
    [Test]
    public async Task Validate_TestableMessageHandlerContext()
    {
        var context = new RecordingHandlerContext();

        var message = new MyMessage();
        await ThrowsTask(() => context.Validate(message));
    }

    [Test]
    public async Task Validate_ValidatingHandlerContext()
    {
        var message = new MyMessage();
        var context = ValidatingContext.Build(message);
        var handler = new MyHandler();
        await ThrowsTask(() => context.Run(handler));
    }

    [Test]
    public Task Valid()
    {
        var message = new MyMessage
        {
            Content = "value"
        };
        var context = ValidatingContext.Build(message);
        var handler = new MyHandler();
        return context.Run(handler);
    }

    class SimpleMessage :
        IMessage;

    class HandlerThatSends :
        IHandleMessages<SimpleMessage>
    {
        public Task Handle(SimpleMessage message, HandlerContext context) =>
            context.SendLocal(new SimpleMessage());
    }

    [Test]
    public async Task Should_throw_for_handle()
    {
        var message = new SimpleMessage();
        var context = ValidatingContext.Build(message);
        var handler = new HandlerThatSends();
        await ThrowsTask(() => handler.Handle(message, context));
    }

    [Test]
    public async Task Validate_ValidatingHandlerContext_Static_Run()
    {
        var message = new MyMessage();
        var handler = new MyHandler();
        await ThrowsTask(() => ValidatingContext.Run(handler, message));
    }
}