using System.Text.Json.Serialization;
using Recycler.API.Converters;

namespace Recycler.API;

public class CreateOrderItemDto
{
    [JsonConverter(typeof(CaseForRawMaterialsConverter))]
    public string RawMaterialName { get; set; }
    public int QuantityInKg { get; set; }
}

