namespace Recycler.API.Models.ExternalApiResponses;

public class ThohRawMaterialListResponseDto
{
    public ThohRawMaterialResponseDto[]? Items { get; set; }
}

public class ThohRawMaterialResponseDto
{
  public string? RawMaterialName { get; set; }
  public decimal PricePerKg { get; set; }
  public int QuantityAvailable { get; set; }
}