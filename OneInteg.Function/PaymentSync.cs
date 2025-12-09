using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using OneInteg.Server.Domain.Services;
using OneInteg.Server.Services;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace OneInteg.Function;

public class PaymentSync
{
    private readonly ILogger _logger;
    private readonly ISubscriptionService _subscriptionService;
    private readonly ITenantService _tenantService;
    private readonly ICustomerService _customerService;
    private readonly IPaymentProvider _paymentProvider;
    private readonly IHttpClientFactory _httpClientFactory;

    public PaymentSync(ILoggerFactory loggerFactory, ISubscriptionService subscriptionService,
        ITenantService tenantService, ICustomerService customerService, 
        [FromKeyedServices(PaymentProviderType.MercadoPago)] IPaymentProvider paymentProvider,
        IHttpClientFactory httpClientFactory)
    {   
        _logger = loggerFactory.CreateLogger<PaymentSync>();
        _subscriptionService = subscriptionService;
        _tenantService = tenantService;
        _customerService = customerService;
        _paymentProvider = paymentProvider;
        _httpClientFactory = httpClientFactory;
    }

    [Function("PaymentSync")]
    public async Task Run([TimerTrigger("*/5 0 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);

        var subscriptions = _subscriptionService.GetSubscriptionsSyncPending(DateTime.UtcNow).Result;
        // iterate through the subscription's payments and sync them to OneInteg
        foreach (var subscription in subscriptions)
        {
            _logger.LogInformation("Processing subscription: {subscriptionId}", subscription.Reference);

            try
            {
                var customer = _customerService.Find(c => c.CustomerId == subscription.CustomerId && c.TenantId == subscription.TenantId).Result.FirstOrDefault();
                var tenant = _tenantService.Find(t => t.TenantId == subscription.TenantId).Result.FirstOrDefault();

                if (customer is null || tenant is null)
                {
                    _logger.LogWarning("Customer or Tenant not found for subscription: {subscriptionId}", subscription.Reference);
                    continue;
                }

                var _subscription = _paymentProvider.HandleSubscriptionPayment(new Server.Domain.Entities.Subscription
                {
                    SubscriptionId = subscription.SubscriptionId,
                    TenantId = subscription.TenantId,
                    CustomerId = subscription.CustomerId,
                    PaymentMethodId = subscription.PaymentMethodId,
                    Reference = subscription.Reference,
                    PlanReference = subscription.PlanReference,
                    StartDate = subscription.StartDate,
                    EndDate = subscription.EndDate,
                    NextPaymentDate = subscription.NextPaymentDate,
                    CreateAt = subscription.CreateAt,
                    UpdateAt = subscription.UpdateAt

                }).Result;

                if (_subscription is null)
                {
                    _logger.LogWarning("No subscription returned from payment provider for subscription: {subscriptionId}", subscription.Reference);
                    continue;
                }
                // hack: this should be done via background job and should be implmented in a service, but for now we will do it here
                if (string.IsNullOrEmpty(tenant.Settings.WebhookUrl))
                {
                    _logger.LogWarning("No webhook URL configured for tenant: {tenantId}", tenant.TenantId);
                    continue;
                }

                using var client = _httpClientFactory.CreateClient();

                client.DefaultRequestHeaders.Add("x-api-key", tenant.Settings.WebhookSecretKey);

                var data = new
                {
                    UserEmail = customer.Email,
                    _subscription.PlanReference,
                    SubscriptionReference = _subscription.Reference,
                    Period = new
                    {
                        _subscription.StartDate,
                        _subscription.EndDate,
                        _subscription.NextPaymentDate
                    }
                };

                var response = client.PostAsJsonAsync(tenant.Settings.WebhookUrl, data, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
                }).Result;

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully sent webhook for subscription: {subscriptionId}", subscription.Reference);

                    _subscriptionService.Update(new Server.DataAccess.Subscription
                    {
                        _id = subscription._id,
                        SubscriptionId = _subscription.SubscriptionId,
                        TenantId = _subscription.TenantId,
                        CustomerId = _subscription.CustomerId,
                        PaymentMethodId = _subscription.PaymentMethodId,
                        Reference = _subscription.Reference,
                        PlanReference = _subscription.PlanReference,
                        StartDate = _subscription.StartDate,
                        EndDate = _subscription.EndDate,
                        NextPaymentDate = _subscription.NextPaymentDate,
                        CreateAt = _subscription.CreateAt,
                        UpdateAt = _subscription.UpdateAt
                    }).Wait();

                    _logger.LogInformation("Successfully synced payments for subscription: {subscriptionId}", subscription.Reference);
                }
                else
                {
                    _logger.LogWarning("Failed to send webhook for subscription: {subscriptionId}. Status Code: {statusCode}", subscription.Reference, response.StatusCode);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error syncing payments for subscription: {subscriptionId}", subscription.Reference);
            }
        }

        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}