using System.ComponentModel.DataAnnotations.Schema;

namespace Recycler.API.Models
{
    [Table("Machines")]
    public class ReceivedMachineDto
    {
        public int Id { get; set; } 
        public int MachineId { get; set; } 
        public DateTime ReceivedAt { get; set; }
        public bool IsOperational { get; set; } 
    }
}
