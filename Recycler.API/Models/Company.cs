using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API.Models;

[Table("Companies")]
public class Company
{
    public int Id { get; set; }
    
    public int RoleId { get; set; }
    public Role Role { get; set; }

    public string Name { get; set; }
    
    public int KeyId { get; set; } 
}