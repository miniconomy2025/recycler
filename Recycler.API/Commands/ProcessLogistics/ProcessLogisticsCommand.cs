using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
    public class ProcessLogisticsCommand : IRequest<LogisticsResponseDto>
    {
        public string? Id { get; set; } 
        public string? Type { get; set; } 
        public List<LogisticsItemDto>? Items { get; set; }
    }
}