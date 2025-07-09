using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("OrderStatus")]
public class OrderStatus
{
    public int Id { get; set; }
    
    public string? Name { get; set; }
}
