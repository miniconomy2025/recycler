using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models.ExternalApiResponses
{
    public class CreatePickupRequestResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = default!;
        public int? PickupRequestId { get; set; }
        public decimal? Cost { get; set; }
        public string? PaymentReferenceId { get; set; }
        public string? BulkLogisticsBankAccount { get; set; }
        public string? Status { get; set; }
        public string? StatusCheckUrl { get; set; }
    }

}