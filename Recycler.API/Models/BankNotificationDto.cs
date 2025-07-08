namespace Recycler.API;

public class BankNotificationDto
{
    public string transaction_number { get; set; } = default!;
    public string status { get; set; } = default!;
    public double amount { get; set; }
    public double timestamp { get; set; }
    public string description { get; set; } = default!;
    public string from { get; set; } = default!;
    public string to { get; set; } = default!;
}