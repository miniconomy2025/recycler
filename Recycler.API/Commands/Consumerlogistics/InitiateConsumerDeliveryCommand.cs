using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class InitiateConsumerDeliveryCommand : IRequest<ConsumerLogisticsDeliveryResponseDto>
    {
        public string CompanyName { get; set; }
        public int Quantity { get; set; }
        public string Recipient { get; set; }
        public string ModelName { get; set; }
    }
}