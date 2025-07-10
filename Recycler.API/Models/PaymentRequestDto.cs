public class PaymentRequestDto
{
    public string transaction_number { get; set; } = Guid.NewGuid().ToString();
    public string status { get; set; } = "initiated";
    public decimal amount { get; set; }
    public double timestamp { get; set; }
    public string description { get; set; } = string.Empty;
    public string from { get; set; } = string.Empty;
    public string to { get; set; } = string.Empty;
}
