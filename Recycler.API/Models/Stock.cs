namespace Recycler.API.Models
{
    public class StockSet
    {
        public List<StockItem> RawMaterials { get; set; }
        public List<StockItem> Phones { get; set; }
    }

    public class StockItem
    {
        public String Name { get; set; }
        public int Quantity { get; set; }
        public String Unit { get; set; }
    }
}