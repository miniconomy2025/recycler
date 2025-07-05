using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Dto;
using Recycler.API.Models;

namespace Recycler.API.Queries.GetAvailablePhones
{
    public class GetAvailablePhonesQuery : IRequest<List<PhoneInventoryDto>>
    {
        public int? PhoneBrandId { get; set; }
        public string? ModelFilter { get; set; }
    }

}