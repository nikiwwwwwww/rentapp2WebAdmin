using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Mesage
    {
        public int IdMesages { get; set; }
        public int? ChatId { get; set; }
        public string? Mesage1 { get; set; }
        public DateTime? SendDate { get; set; }
        public string? StatusId { get; set; }
        public int? SenderUserId { get; set; }

    }
}
