namespace Recycler.API.Queries.GetMaterials;

public class RawMaterialDto
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public float AvailableQuantityInKg { get; set; }
    public decimal Price { get; set; }
}
