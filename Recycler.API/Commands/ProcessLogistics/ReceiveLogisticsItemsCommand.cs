
using MediatR;
using Recycler.API.Models;

namespace Recycler.API.Commands
{
     public class ReceiveLogisticsItemsCommand : IRequest<Unit> 
    {
        public List<LogisticsItemDto>? ItemsToReceive { get; set; }
    }
}