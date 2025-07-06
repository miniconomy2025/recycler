using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recycler.API.Models;

namespace Recycler.API.Dto
{
  public class PhoneInventoryDto
    {
        public int PhoneId { get; set; }
        public string Model { get; set; }
        public string BrandName { get; set; }
        public int AvailableQuantity { get; set; }
        public PhoneRecyclingEstimate EstimatedYield { get; set; }
    }
}