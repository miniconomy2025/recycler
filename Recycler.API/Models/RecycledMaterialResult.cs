using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models
{
    public class RecycledMaterialResult
    {
        public int MaterialId { get; set; }
        public string MaterialName { get; set; }
        public double QuantityInKg { get; set; }
        public DateTime RecycledDate { get; set; }
        public string SourcePhoneModels { get; set; }
    }
}