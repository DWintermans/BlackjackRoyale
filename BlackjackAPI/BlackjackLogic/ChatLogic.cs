using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.ViewModels;
using Group = BlackjackCommon.Models.Group;
using Player = BlackjackCommon.Models.Player;

namespace BlackjackLogic
{
	public class ChatLogic : IChatLogic
	{
		private const int MaxGroupSize = 4;

		public event Func<Player, string, NotificationType, ToastType?, Task>? OnNotification;
		public event Func<Player, int, string, MessageType, Task>? OnMessage;
		public event Func<Player, int, string, Task>? OnPrivateMessage;

		public async Task HandleChatAction(Player player, dynamic message)
		{
			switch (message.action.ToString())
			{
				case "send_message":
					await SendMessage(player, message.receiver.ToString(), message.message.ToString());
					break;

				case "delete_message":
					//moderator only
					//await DeleteMessage();
					break;
				default:
					await OnNotification?.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
					break;
			}
		}

		public async Task SendMessage(Player player, string receiver, string chatMessage)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (receiver.ToString().ToUpper() == "GLOBAL") //global message
			{
				if (group == null)
				{
					await SendMessageGlobally(player, chatMessage);
				}
				else
				{
					await OnNotification?.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);				
				}
			}
			else if (receiver.ToString().ToUpper() == "GROUP") //group message
			{
				if (group != null)
				{
					await SendMessageToGroup(player, group, chatMessage);
				}
				else
				{
					await OnNotification?.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);	
				}
			}
			else if (int.TryParse(receiver.ToString(), out int receiver_id)) //private message
			{
				//cant send private message to yourself
				if (player.User_ID != receiver_id)
				{
					await OnPrivateMessage?.Invoke(player, receiver_id, chatMessage);
				}
			}
			else
			{	
				await OnNotification?.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);				
			}
		}

		public async Task SendMessageToGroup(Player player, Group group, string chatMessage)
		{
			foreach (var member in group.Members)
			{
				await OnMessage?.Invoke(player, member.User_ID, chatMessage, MessageType.GROUP);
			}
		}

		public async Task SendMessageGlobally(Player player, string chatMessage)
		{
			foreach (var userEntry in SharedData.Players)
			{
				Player user = userEntry.Value;

				bool isInGroup = SharedData.GetGroupForPlayer(user) != null;

				if (!isInGroup)
				{
					await OnMessage?.Invoke(player, user.User_ID, chatMessage, MessageType.GLOBAL);
				}
			}
		}
	}
}