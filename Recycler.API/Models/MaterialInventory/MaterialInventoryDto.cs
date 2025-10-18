using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models.MaterialInventory
{
    public class MaterialInventoryDto
    {
        public required string MaterialName { get; set; }
        public double AvailableQuantityInKg { get; set; }
    }
}