using MediatR;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
    public class ProcessConsumerDropOffCommand : IRequest<ConsumerLogisticsDropOffResponseDto>
    {
        public string Status { get; set; }
        public string ModelName { get; set; }
        public int Quantity { get; set; }
    }
}