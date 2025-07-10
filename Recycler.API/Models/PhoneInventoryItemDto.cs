
namespace Recycler.API.Models

{
    public class PhoneInventoryItemDto
    {
        public int PhoneId { get; set; } 
        public string? Model { get; set; } 
        public string? BrandName { get; set; } 
        public int AvailableQuantity { get; set; } 
       
    }
}