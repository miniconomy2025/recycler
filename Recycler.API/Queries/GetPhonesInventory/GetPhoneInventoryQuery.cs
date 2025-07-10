using MediatR;
using RecyclerApi.Models;
using System.Collections.Generic;

namespace RecyclerApi.Queries
{
    public class GetPhoneInventoryQuery : IRequest<List<PhoneInventoryItemDto>>
    {
        
    }
}