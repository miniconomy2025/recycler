using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("RawMaterial")]
public class RawMaterial
{
    public int Id { get; set; }
    
    public string Name { get; set; }
    
    public decimal Price { get; set; }
}
