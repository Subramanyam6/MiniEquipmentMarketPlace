namespace MiniEquipmentMarketplace.Models
{
    public class Equipment
    {
        public int EquipmentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int VendorId { get; set; }
        public Vendor? Vendor { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
