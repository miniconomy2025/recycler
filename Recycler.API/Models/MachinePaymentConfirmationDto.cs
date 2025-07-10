using System.Text.Json.Serialization;

namespace Recycler.API.Models;

public class MachinePaymentConfirmationDto
{
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    [JsonPropertyName("totalWeight")]
    public decimal TotalWeight { get; set; }
}
