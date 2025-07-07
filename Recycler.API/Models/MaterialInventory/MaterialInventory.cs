using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("MaterialInventory")]
public class MaterialInventory
{
    public int Id { get; set; }
    
    public int MaterialId { get; set; }
    
    public double AvailableQuantityInKg { get; set; }
}

