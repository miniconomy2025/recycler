using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Dto
{
    public class PhonePartRatioDto
    {
        public int PartsPerPhone { get; set; }
        public int MaterialPerPart { get; set; }
        public required string MaterialName { get; set; }
    }
}