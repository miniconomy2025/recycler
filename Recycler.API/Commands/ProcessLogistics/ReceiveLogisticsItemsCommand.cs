using MediatR;
using RecyclerApi.Models;
using System.Collections.Generic;

namespace RecyclerApi.Commands
{
    public class ReceiveLogisticsItemsCommand : IRequest<Unit> 
    {
        public List<LogisticsItemDto> ItemsToReceive { get; set; }
    }
}