namespace Recycler.API.Queries.GetMaterials;

public class RawMaterialDto
{
    public required string Name { get; set; }
    public double AvailableQuantityInKg { get; set; }
    public decimal PricePerKg { get; set; }
}
