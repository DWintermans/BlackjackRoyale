using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IChatLogic
	{
		event Func<Player, string, NotificationType, ToastType?, Task> OnNotification;
		event Func<Player, int, string, MessageType, Task> OnMessage;
		event Func<Player, int, string, Task> OnPrivateMessage;

		Task HandleChatAction(Player player, dynamic message);
	}
}
