using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class ProcessConsumerDeliveryNotificationCommand : IRequest<ConsumerLogisticsDeliveryResponseDto> 
    {
        public string Status { get; set; } = "";
        public string ModelName { get; set; } = "";
        public int Quantity { get; set; }
    }
}