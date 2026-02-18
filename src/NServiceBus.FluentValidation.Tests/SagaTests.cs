public class SagaTests
{
    [Test]
    public async Task Ensure_saga_data_is_added_to_context()
    {
        var message = new MyMessage
        {
            Content = "a"
        };

        var handlerContext = ValidatingContext.Build(message);
        var sagaData = new MySaga.MySagaData();
        var saga = new MySaga
        {
            Data = sagaData
        };
        await handlerContext.Run(saga);
        await Assert.That(handlerContext.SagaData).IsEqualTo(sagaData);
    }
}
