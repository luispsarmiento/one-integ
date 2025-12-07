using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace OneInteg.Function;

public class PaymentSync
{
    private readonly ILogger _logger;

    public PaymentSync(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<PaymentSync>();
    }

    [Function("PaymentSync")]
    public void Run([TimerTrigger("* * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("C# Timer trigger function executed at: {executionTime}", DateTime.Now);
        
        if (myTimer.ScheduleStatus is not null)
        {
            _logger.LogInformation("Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
        }
    }
}