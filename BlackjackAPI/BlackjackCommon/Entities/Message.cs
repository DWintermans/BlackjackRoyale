using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.Message
{
	[Table("message")]
	public class Message
	{
		[Key] 
		[Column("message_id")]
		public int MessageId { get; set; }

		[Column("message_sender")]
		public int MessageSender { get; set; }

		[Column("message_receiver")]
		public int? MessageReceiver { get; set; } 

		[Column("message_group")]
		[StringLength(50)]
		public string? MessageGroup { get; set; } 

		[Column("message_content")]
		public string MessageContent { get; set; } 

		[Column("message_datetime")]
		public DateTime MessageDateTime { get; set; }

		[Column("message_deleted")]
		public bool MessageDeleted { get; set; }

		[NotMapped]
		public string SenderUserName { get; set; }
		
		[NotMapped]
		public string ReceiverUserName { get; set; }	
	}
}
