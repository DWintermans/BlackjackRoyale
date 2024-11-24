using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BlackjackCommon.Entities.Message
{
	[Table("message")]
	public class Message
	{
		[Key] 
		[Column("message_id")]
		public int message_id { get; set; }

		[Column("message_sender")]
		public int message_sender { get; set; }

		[Column("message_receiver")]
		public int? message_receiver { get; set; } 

		[Column("message_group")]
		[StringLength(50)]
		public string? message_group { get; set; } 

		[Column("message_content")]
		public string message_content { get; set; } 

		[Column("message_datetime")]
		public DateTime message_datetime { get; set; }

		[Column("message_deleted")]
		public bool message_deleted { get; set; }
	}
}
