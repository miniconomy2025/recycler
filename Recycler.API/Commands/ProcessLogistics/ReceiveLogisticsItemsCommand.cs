
using MediatR;
using Recycler.API.Models;
using RecyclerApi.Models;

namespace RecyclerApi.Commands
{
     public class ReceiveLogisticsItemsCommand : IRequest<Unit> 
    {
        public List<LogisticsItemDto> ItemsToReceive { get; set; }
    }
}