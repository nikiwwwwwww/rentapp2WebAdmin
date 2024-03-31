using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Photo
    {
        public int IdPhoto { get; set; }
        public string? photopath { get; set; }
        public int? ProductId { get; set; }

    }
}
