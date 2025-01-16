using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace BlackjackCommon.ViewModels
{
    public class MessageListModel
    {
        public int message_id { get; set; }

        public int message_sender { get; set; }

        public int? message_receiver { get; set; }

        public string? message_content { get; set; }

        public DateTime? message_datetime { get; set; }

        public bool message_deleted { get; set; }

        public string? sender_username { get; set; }

        public string? receiver_username { get; set; }
    }
}
