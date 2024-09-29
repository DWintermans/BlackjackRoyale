namespace BlackjackService
{
	public class NotificationModel
	{
		public required NotificationType Type { get; set; }
		public required string Message { get; set; }
		public ToastType? ToastType { get; set; }
	}

	public enum NotificationType
	{
		GROUP,
		GAME,
		TOAST,
	}

	public enum ToastType
	{
		INFO,
		SUCCESS,
		WARNING,
		ERROR,
		DEFAULT,
	}
}