namespace RecyclerApi.Models
{
    public class MachineOrderRequestDto
    {
        public string? machineName { get; set; }
        public int quantity { get; set; }

    }

    public class MachineOrderResponseDto
    {
        public string? Message { get; set; }
        public int OrderId { get; set; }
        public string? BankAccount { get; set; }
    }
}