using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Recycler.API.Models.ExternalApiResponses;

namespace Recycler.API.Commands.CreatePickupRequest
{
    public class CreatePickupRequestCommand : IRequest<CreatePickupRequestResponse>
    {
        public string originalExternalOrder { get; set; } = default!;
        public string originCompany { get; set; } = default!;
        public string destinationCompany { get; set; } = default!;
        public List<PickupItem> items { get; set; } = new();
    }

    public class PickupItem
    {
        public string itemName { get; set; } = default!;
        public int quantity { get; set; }
    }
}