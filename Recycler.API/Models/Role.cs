using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API.Models;

[Table("Role")]
public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}