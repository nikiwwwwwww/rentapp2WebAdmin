using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Region
    {
        public int IdRegion { get; set; }
        public int? countryId { get; set; }
        public string? Name { get; set; }
    }
}
