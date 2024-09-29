namespace BlackjackService
{
	public class MessageModel
	{
		public required MessageType Type { get; set; }
		public required string SenderName { get; set; }
		public required int Sender { get; set; }
		public required int Receiver { get; set; }
		public required string Message { get; set; }
		public required DateTime Datetime { get; set; }
	}

	public enum MessageType
	{
		PRIVATE,
		GLOBAL,
		GROUP,
	}
}