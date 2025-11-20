using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using OneInteg.Server.Domain.Repositories;
using OneInteg.Server.Domain.Services;
using OneInteg.Server.IoCConfig;
using OneInteg.Server.Services;
using System.Net.Mail;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddCustomMongoDbService();

builder.Services.AddServiceAndRepositories();
builder.Services.AddPaymentProviders();

builder.Services.AddHttpClient();

builder.Services.AddSession(options =>
{
    options.Cookie.Name = ".1Integ.Session";
    options.IdleTimeout = TimeSpan.FromSeconds(300);
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors();

app.UseSession();

app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");


RouteGroupBuilder link = app.MapGroup("/link");
link.MapGet("/{t_id}/subscription/checkout-url", CheckoutUrl);

RouteGroupBuilder backUrl = app.MapGroup("/back-url");

backUrl.MapGet("/{t_id}/subscription/mp", BackUrlSubscriptionMP);

app.Run();


static async Task<IResult> CheckoutUrl(
    HttpContext contex,
    [FromRoute(Name = "t_id")] Guid tenantId,
    [FromQuery(Name = "customer")] string? customer,
    [FromQuery(Name = "plan_id")] string? planId,
    [FromQuery(Name = "promotion_code")] string? promotionCode,
    //SERVICES
    [FromServices] ITenantRepository tenantRepository,
    [FromServices] ISubscriptionService subscriptionService)
{
    var tenant = (await tenantRepository.Find(doc => doc.TenantId == tenantId)).FirstOrDefault();

    if (tenant == null)
    {
        return Results.Redirect("/not-found");
    }

    contex.Session.SetString("ce", customer);
    var subscriptionLink = await subscriptionService.GetCheckoutUrl(new OneInteg.Server.DataAccess.Customer
    {
        TenantId = tenant.TenantId,
        Email = Encoding.UTF8.GetString(Convert.FromBase64String(customer))
    }, planId, promotionCode);

    if (string.IsNullOrEmpty(subscriptionLink))
    {
        return Results.Redirect("/not-found");
    }

    return Results.Redirect(subscriptionLink);
}

static async Task<IResult> BackUrlSubscriptionMP(
    HttpContext contex, 
    [FromRoute(Name = "t_id")] Guid tenantId, 
    [FromKeyedServices(PaymentProviderType.MercadoPago)] IPaymentProvider paymentProvider,
    [FromServices] ITenantRepository tenantRepository,
    [FromServices] IHttpClientFactory httpClientFactory)
{
    Console.WriteLine(contex.Session.GetString("ce"));
    var queryParams = contex.Request.Query;
    var preapprolvaId = queryParams["preapproval_id"];
    Console.WriteLine("Preapproval_id: {0}", preapprolvaId);

    if (string.IsNullOrEmpty(preapprolvaId))
    {
        return TypedResults.BadRequest();
    }

    var customerEmail = Encoding.UTF8.GetString(
        Convert.FromBase64String(contex.Session.GetString("ce")));

    var subscription = await paymentProvider.HandleBackUrlSubscription(tenantId, preapprolvaId, customerEmail);

    var tenant = tenantRepository.Find(t => t.TenantId == tenantId).Result.First();

    if (!string.IsNullOrEmpty(tenant.Settings.WebhookUrl))
    {
        using var client = httpClientFactory.CreateClient();

        client.DefaultRequestHeaders.Add("x-api-key", tenant.Settings.WebhookSecretKey);
        
        var data = new
        {
            UserEmail = customerEmail,
            subscription.PlanReference,
            SubscriptionReference = subscription.Reference,
            Period = new
            {
                subscription.StartDate,
                subscription.EndDate,
                subscription.NextPaymentDate
            }
        };

        await client.PostAsJsonAsync(tenant.Settings.WebhookUrl, data, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
        });
    }

    if (!string.IsNullOrEmpty(tenant.Settings.BackUrl))
    {
        return Results.Redirect(tenant.Settings.BackUrl);
    }

    return Results.Redirect("/subscription-started");
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}