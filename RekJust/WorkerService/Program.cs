#pragma warning disable SKEXP0070 // Ollama connector jest w wersji preview Semantic Kernel

using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using WorkerService.Consumers;
using WorkerService.Data;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddKernel()
    .AddOllamaChatCompletion(
        modelId: builder.Configuration["Ollama:Model"] ?? "llama3.2",
        endpoint: new Uri(builder.Configuration["Ollama:Endpoint"] ?? "http://ollama:11434"));

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<PromptCreatedConsumer>();

    x.UsingRabbitMq((ctx, cfg) =>
    {
        cfg.Host(builder.Configuration["RabbitMQ:Host"] ?? "localhost", "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"] ?? "guest");
            h.Password(builder.Configuration["RabbitMQ:Password"] ?? "guest");
        });

        // Exponential backoff: 3 próby z opóźnieniami 2s, 4s, 8s.
        // Chroni przed chwilowymi błędami LLM (timeout, przeciążenie modelu).
        cfg.UseMessageRetry(r => r.Exponential(3,
            TimeSpan.FromSeconds(2),
            TimeSpan.FromSeconds(30),
            TimeSpan.FromSeconds(2)));

        cfg.ConfigureEndpoints(ctx);
    });
});

var host = builder.Build();
host.Run();
