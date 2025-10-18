using MediatR;
using Recycler.API.Models;
using System.Collections.Generic;

namespace Recycler.API.Queries.GetPhonesInventory
{
    public class GetPhonesInventoryQuery : IRequest<List<PhoneInventoryItemDto>>
    {
        public string? PhoneBrandId { get; set; }
        
    }
}