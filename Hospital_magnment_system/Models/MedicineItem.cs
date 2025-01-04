namespace Hospital_magnment_system.Models
{
    public class MedicineItem
    {
        public int MedicineID { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal Total => Quantity * Price;
    }
} 