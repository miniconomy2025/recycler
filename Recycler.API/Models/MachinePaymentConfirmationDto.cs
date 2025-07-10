using System.Text.Json.Serialization;

namespace Recycler.API.Models;

public class MachinePaymentConfirmationDto
{
    [JsonPropertyName("orderNumber")]
    public string OrderNumber { get; set; } = string.Empty;
}
