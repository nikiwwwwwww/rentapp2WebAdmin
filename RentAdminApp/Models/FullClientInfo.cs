namespace RentAdminApp.Models
{
    public class FullClientInfo
    {
        public Client? client { get; set; }
        public List<Booking>? booking { get; set; }
        public List<Product>? product { get; set; }
    }
}
