using MassTransit;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Moq;
using PromptApi.Services;

namespace PromptApi.Tests.Integration;

/// <summary>
/// Fabryka aplikacji ASP.NET Core uruchamiana w pamięci na potrzeby testów HTTP.
/// Podmienia IPromptService na mock, żeby testować wyłącznie warstwę HTTP (routing, walidacja, kody odpowiedzi).
/// UseEnvironment("Testing") powoduje pominięcie migracji EF Core w Program.cs.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IPromptService> PromptServiceMock { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IPromptService>();
            services.AddScoped(_ => PromptServiceMock.Object);

            // MassTransit bus nie startuje — brak połączenia z RabbitMQ w testach.
            services.RemoveAll<IHostedService>();

            services.RemoveAll<IPublishEndpoint>();
            services.AddSingleton(Mock.Of<IPublishEndpoint>());
        });
    }
}
