using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class ProcessConsumerDeliveryNotificationCommand : IRequest<ConsumerLogisticsDeliveryResponseDto> 
    {
        public string Status { get; set; }
        public string ModelName { get; set; }
        public int Quantity { get; set; }
    }
}