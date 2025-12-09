using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OneInteg.Server.IoCConfig;

var builder = FunctionsApplication.CreateBuilder(args);

builder.Services.AddCustomMongoDbService();

builder.Services.AddServiceAndRepositories();
builder.Services.AddPaymentProviders();

builder.Services.AddHttpClient();

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

builder.Build().Run();
