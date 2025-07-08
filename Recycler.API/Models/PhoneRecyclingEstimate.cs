using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models
{
     public class PhoneRecyclingEstimate
    {
        public int PhoneId { get; set; }
        public required string PhoneModel { get; set; }
        public required string BrandName { get; set; }
        public Dictionary<string, double> EstimatedMaterials { get; set; } = new Dictionary<string, double>();
        public double TotalEstimatedQuantity { get; set; }
    }
}