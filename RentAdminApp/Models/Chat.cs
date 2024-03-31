using System;
using System.Collections.Generic;

namespace RentAdminApp.Models
{
    public partial class Chat
    {
      

        public int IdChat { get; set; }
        public int? SenderUserId { get; set; }
        public int? ReceiverUserId { get; set; }

    }
}
