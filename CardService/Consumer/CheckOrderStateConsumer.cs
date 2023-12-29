using MassTransit;
using Messages.Order.Command;
using Messages.Order.Event;
using OpenTelemetry.Trace;
using Microsoft.Extensions.Logging; // Add this using statement
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace CardService.Consumer
{
    public class CheckOrderStateConsumer : IConsumer<ICheckOrderStateCommand>
    {
        private readonly ILogger<CheckOrderStateConsumer> _logger;
        private readonly TracerProvider _tracerProvider;

        public CheckOrderStateConsumer(ILogger<CheckOrderStateConsumer> logger, TracerProvider tracerProvider)
        {
            _logger = logger;
            _tracerProvider = tracerProvider;
        }

        public async Task Consume(ConsumeContext<ICheckOrderStateCommand> context)
        {
            try
            {
                // Log the start of the consumer processing
                _logger.LogInformation("Check Order State Consumer started - OrderId: {OrderId}", context.Message.OrderId);

                if (context.Message.IsCanceled)
                {
                    await context.Publish<IOrderCanceledEvent>(new
                    {
                        context.Message.OrderId,
                        ExceptionMessage = "order canceled"
                    });

                    // Log the step
                    _logger.LogInformation("Order Canceled - OrderId: {OrderId}", context.Message.OrderId);
                }
                else
                {
                    await context.Publish<IOrderFinishedEvent>(new
                    {
                        context.Message.OrderId
                    });

                    // Log the step
                    _logger.LogInformation("Order Finished - OrderId: {OrderId}", context.Message.OrderId);
                }
            }
            catch (Exception ex)
            {
                // Capture and log the exception details including the source code line
                var exceptionDetails = new
                {
                    Message = ex.Message,
                    StackTrace = ex.StackTrace
                };

                _logger.LogError(ex, "An exception occurred - OrderId: {OrderId}. Details: {@ExceptionDetails}", context.Message.OrderId, exceptionDetails);

                // Additionally, capture the exception details in the OpenTelemetry span
                var span = _tracerProvider.GetTracer("http-client")
                    .StartActiveSpan("request");

                span.SetAttribute("exception.message", ex.Message);
                span.SetAttribute("exception.stackTrace", ex.StackTrace);

                // Rethrow the exception if needed
                throw;
            }

            // Log the end of the consumer processing
            _logger.LogInformation("Check Order State Consumer completed - OrderId: {OrderId}", context.Message.OrderId);

            await Task.CompletedTask;
        }
    }
}
