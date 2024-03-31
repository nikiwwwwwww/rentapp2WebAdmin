namespace RentAdminApp.Models
{
    public partial class FullProductParametrInfo
    {
        public Product? product { get; set; }
        public Category? category { get; set; }
        public City? city { get; set; }
        public Client? сlient { get; set; }
        public List<ProductParameter>? productparametr { get; set; }
        public List<Parametr>? parameter{ get; set; }
        public List<AtributsParam>? attributeParams { get; set; }
        public List<BookingPrice>? bookingPrices { get; set; }
        public List<Photo>? photo { get; set; }
    }
}

