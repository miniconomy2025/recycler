using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("Log")]
public class Log()
{
    public int Id { get; set; }

    public string RequestSource { get; set; } = "";
    
    public string RequestEndpoint { get; set; } = "";
    
    
    [Column(TypeName = "jsonb")]
    public string RequestBody { get; set; } = "";
    
    
    [Column(TypeName = "jsonb")]
    public string Response { get; set; } = "";
    
    public DateTime Timestamp { get; set; } = DateTime.Now;
}