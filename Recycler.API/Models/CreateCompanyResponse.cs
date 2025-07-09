namespace RecyclerApi.Models
{
    public class CreateCompanyResponse
    {
        public int CompanyId { get; set; } 
        public Guid CompanyNumber { get; set; } 
        public string? Name { get; set; }
        public string? Role { get; set; } 
    }
}
