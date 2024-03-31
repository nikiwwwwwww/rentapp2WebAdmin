using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Product
    {
        public int IdProduct { get; set; }
        public string? NameProduct { get; set; }
        public string? Latitude { get; set; }
        public string? Longitude { get; set; }
        public string? Descriptions { get; set; }
        public bool? Statuss { get; set; }
        public int? CategoryId { get; set; }
        public int? CityId { get; set; }
        public int? ClientId { get; set; }

    }
}
