using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Recycler.API.Dto;

namespace Recycler.API.Models
{
   public class RecyclingEligibilityResult
    {
        public bool IsEligible { get; set; }
        public int TotalPhonesAvailable { get; set; }
        public int MinimumRequired { get; set; } = 1000;
        public string Message { get; set; }
        public List<PhoneInventoryDto> AvailablePhones { get; set; } = new List<PhoneInventoryDto>();
        public Dictionary<string, double> TotalEstimatedYield { get; set; } = new Dictionary<string, double>();
    }
}