using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class City
    {
        public int IdCity { get; set; }
        public int? regionId { get; set; }
        public string? Name { get; set; }
    }
}
