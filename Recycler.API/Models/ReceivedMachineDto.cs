namespace RecyclerApi.Models
{
    public class ReceivedMachineDto
    {
        public int Id { get; set; } 
        public int MachineId { get; set; } 
        public DateTime ReceivedAt { get; set; }
        public string Status { get; set; } 
    }
}
