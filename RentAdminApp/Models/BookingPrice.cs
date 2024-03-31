using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class BookingPrice
    {
        public int IdBookingPrice { get; set; }
        public int? ProductId { get; set; }
        public int? HourlyTime { get; set; }
        public int? DailyTime { get; set; }
        public decimal? Price { get; set; }

    }
}
