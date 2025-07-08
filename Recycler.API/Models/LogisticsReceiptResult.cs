using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Recycler.API.Models
{
    public class LogisticsReceiptResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int PhonesReceived { get; set; }
        public List<string> ReceivedPhoneModels { get; set; } = new();
        public RecyclingResult? RecyclingResult { get; set; }
    }
}