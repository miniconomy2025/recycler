using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API;

[Table("Log")]
public class Log()
{
    public int Id { get; set; }

    public string RequestSource { get; set; }
    
    public string RequestEndpoint { get; set; }
    
    public string RequestBody { get; set; }
    
    public string Response { get; set; }
}