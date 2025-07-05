using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models
{
       public class RecyclingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<RecycledMaterialResult> RecycledMaterials { get; set; } = new List<RecycledMaterialResult>();
        public double TotalMaterialsRecycled { get; set; }
        public Guid? OrderNumber { get; set; }
        public int PhonesProcessed { get; set; }
        public DateTime ProcessingDate { get; set; }
    }

}