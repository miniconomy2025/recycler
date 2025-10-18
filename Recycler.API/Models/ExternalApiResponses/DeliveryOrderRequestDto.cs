namespace Recycler.API.Models.ExternalApiRequests;

public class DeliveryOrderRequestDto
{
    public string companyName { get; set; } = "recycler";
    public int quantity { get; set; }
    public string recipient { get; set; } = "recycler";
    public string modelName { get; set; } = string.Empty;
}