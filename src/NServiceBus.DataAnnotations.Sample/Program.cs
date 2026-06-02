var builder = Host.CreateApplicationBuilder();
var configuration = new EndpointConfiguration("DataAnnotationsValidationSample");
configuration.UsePersistence<LearningPersistence>();
configuration.UseTransport<LearningTransport>();
configuration.UseDataAnnotationsValidation(outgoing: false);
configuration.UseSerialization<SystemJsonSerializer>();

builder.Services.AddNServiceBusEndpoint(configuration);
using var host = builder.Build();
await host.StartAsync();
var session = host.Services.GetRequiredService<IMessageSession>();

await session.SendLocal(new MyMessage
{
    Content = "sd"
});
await session.SendLocal(new MyMessage());

Console.WriteLine("Press any key to stop program");
Console.Read();
await host.StopAsync();