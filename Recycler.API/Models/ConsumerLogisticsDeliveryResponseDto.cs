namespace Recycler.API.Models
{
    public class ConsumerLogisticsDeliveryResponseDto
    {
        public Guid ReferenceNo { get; set; }
        public decimal Amount { get; set; } 
        public string AccountNumber { get; set; }
    }
}