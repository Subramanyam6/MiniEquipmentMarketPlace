namespace MiniEquipmentMarketplace.Models
{
    public class Vendor
    {
        public int VendorId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ICollection<Equipment> Equipments { get; set; } = new List<Equipment>();
    }
}
