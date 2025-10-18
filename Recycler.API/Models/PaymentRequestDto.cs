public class PaymentRequestDto
{
    public decimal amount { get; set; }
    public string description { get; set; } = string.Empty;
    public string to_account_number { get; set; } = string.Empty;
    public string to_bank_name { get; set; } = "commercial-bank";
}
