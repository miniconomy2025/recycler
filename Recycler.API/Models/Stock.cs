namespace Recycler.API.Models
{
    public class StockSet
    {
        public List<StockItem>? RawMaterials { get; set; } = [];
        public List<StockItem> Phones { get; set; } = [];
        public int TotalOrders { get; set; }           
        public int CompletedOrders { get; set; }      
        public int PendingOrders { get; set; }  
    }

    public class StockItem
    {
        public string Name { get; set; } = "";
        public int Quantity { get; set; }
        public string Unit { get; set; } = "";
    }
}