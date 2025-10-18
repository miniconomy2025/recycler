namespace Recycler.API.Models
{
    public class LogisticsRequestDto
    {
        public string? Id { get; set; } 
        public string? Type { get; set; } 
        public List<LogisticsItemDto>? Items { get; set; }
    }
}