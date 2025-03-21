using CloudStorage.Shared.Commons;
using CloudStorage.Shared.Commons.Configurations;
using CloudStorage.Worker.SecretsStorage.Configurations;
using CloudStorage.Worker.SecretsStorage.Services;

namespace CloudStorage.Worker.SecretsStorage;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddControllers();
        builder.Services.AddGrpc().AddJsonTranscoding();
        builder.Services.AddHealthChecks();
        await builder.Services.AddSecretsStorageServices(builder.Configuration);
        await builder.Services.AddCoreConfiguration(builder.Configuration);

        var application = builder.Build();
        await using (var scope = application.Services.CreateAsyncScope())
        {
            var secretsService = scope.ServiceProvider.GetRequiredService<SecretsAccessService>();
            await secretsService.SendNewSecretsNotification();
        }
        if (application.Environment.IsDevelopment())
        {
            application.UseSwagger();
            application.UseSwaggerUI();
        }
        application.UseCoreConfiguration();
        application.UseHealthChecks("/health");
        application.MapGrpcService<SecretsServiceImpl>();
        await application.RunAsync();
    }
}