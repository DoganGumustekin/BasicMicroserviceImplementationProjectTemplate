using EventBus.Base.Events;

namespace PaymentService.Api.IntegrationEvents.Events
{
    public class OrderPaymentFailedIntegrationEvent : IntegrationEvent
    {
        public int OrderId { get;}
        public string _errorMessage { get; }
        public OrderPaymentFailedIntegrationEvent(int orderId, string errorMessage)
        {
            _errorMessage = errorMessage;
            OrderId = orderId;
        }
    }
}
