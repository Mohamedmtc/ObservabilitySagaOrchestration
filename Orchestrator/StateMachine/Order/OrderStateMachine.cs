using Automatonymous;
using MassTransit;
using Messages.Order.Event;
using Orchestrator.Presistance;
using Orchestrator.StateMachine.Order.Command;
using Microsoft.Extensions.Logging; // Add this using statement
using System;

namespace Orchestrator.StateMachine.Order
{
    public sealed class OrderStateMachine : MassTransitStateMachine<OrderStateData>
    {
        private readonly ILogger<OrderStateMachine> _logger; // Add a logger field

        public State? OrderStarted { get; private set; }
        public State? OrderCancelled { get; private set; }
        public State? OrderFinished { get; private set; }

        public Event<IOrderStartedEvent>? OrderStartedEvent { get; private set; }
        public Event<IOrderCanceledEvent>? OrderCancelledEvent { get; private set; }
        public Event<IOrderFinishedEvent>? OrderFinishedEvent { get; private set; }

        public OrderStateMachine(ILogger<OrderStateMachine> logger) // Inject the logger through the constructor
        {
            _logger = logger; // Initialize the logger field

            Event(() => OrderStartedEvent, x => x.CorrelateById(context => context.Message.OrderId));
            Event(() => OrderCancelledEvent, x => x.CorrelateById(m => m.Message.OrderId));
            Event(() => OrderFinishedEvent, x => x.CorrelateById(m => m.Message.OrderId));
            InstanceState(x => x.CurrentState);

            Initially(
               When(OrderStartedEvent)
                .Then(context =>
                {
                    context.Saga.OrderCreationDateTime = DateTime.Now;
                    context.Saga.OrderId = context.Message.OrderId;
                    context.Saga.PaymentCardNumber = context.Message.PaymentCardNumber;
                    context.Saga.ProductName = context.Message.ProductName;
                    context.Saga.IsCanceled = context.Message.IsCanceled;

                    // Log the step
                    _logger.LogInformation($"Order Started - OrderId: {context.Message.OrderId}");
                })
               .TransitionTo(OrderStarted)
                .Publish(context => new CheckOrderStateCommand(context.Saga)));

            During(OrderStarted,
               When(OrderCancelledEvent)
                   .Then(context =>
                   {
                       context.Saga.Exception = context.Message.ExceptionMessage;
                       context.Saga.OrderCancelDateTime = DateTime.Now;

                       // Log the step
                       _logger.LogInformation($"Order Cancelled - OrderId: {context.Message.OrderId}");
                   })
                    .TransitionTo(OrderCancelled));

            During(OrderStarted,
                When(OrderFinishedEvent)
                    .Then(context =>
                    {
                        context.Saga.OrderFinishedDateTime = DateTime.Now;

                        // Log the step
                        _logger.LogInformation($"Order Finished - OrderId: {context.Message.OrderId}");
                    })
                     .TransitionTo(OrderFinished)
                     .Finalize());

            SetCompletedWhenFinalized();
        }
    }
}
