class MySaga :
    Saga<MySaga.MySagaData>,
    IAmStartedByMessages<MyMessage>
{
    public Task Handle(MyMessage message, HandlerContext context)
    {
        Data.Property = "Value";
        return Task.CompletedTask;
    }

    protected override void ConfigureHowToFindSaga(SagaPropertyMapper<MySagaData> mapper) =>
        mapper.MapSaga(saga => saga.Property)
            .ToMessage<MyMessage>(msg => msg.Content);

    public class MySagaData :
        ContainSagaData
    {
        public string? Property { get; set; }
    }
}