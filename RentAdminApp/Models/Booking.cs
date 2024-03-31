using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Booking
    {
        public int IdBooking { get; set; }
        public DateTime? Startdate { get; set; }
        public DateTime? Enddate { get; set; }
        public int? BookingPriceId { get; set; }
        public int? ClientId { get; set; }
    }
}
