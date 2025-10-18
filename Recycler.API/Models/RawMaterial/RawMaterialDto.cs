namespace Recycler.API;

public class RawMaterialDto
{
    public required string Name { get; set; }
    public double AvailableQuantityInKg { get; set; }
    public decimal PricePerKg { get; set; }
}
