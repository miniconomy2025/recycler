namespace Recycler.API.Models.ExternalApiRequests;

public class DeliveryOrderRequestDto
{
    public string companyName { get; set; } = "Recycler";
    public int quantity { get; set; }
    public string recipient { get; set; } = "Recycler";
    public string modelName { get; set; } = string.Empty;
}