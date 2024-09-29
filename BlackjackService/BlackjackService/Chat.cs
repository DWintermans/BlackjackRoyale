namespace BlackjackService
{
	public class Chat
	{
		public static async Task HandleChatAction(Player player, dynamic message)
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
					await Websocket.SendNotificationToPlayer(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
					break;
			}
		}

		public static async Task SendMessage(Player player, string receiver, string chatMessage)
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
					await Websocket.SendNotificationToPlayer(player, $"As a member of this group, you can only send messages to this group.", NotificationType.TOAST, ToastType.WARNING);
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
					await Websocket.SendNotificationToPlayer(player, $"You are not a member of a group.", NotificationType.TOAST, ToastType.WARNING);
				}
			}
			else if (int.TryParse(receiver.ToString(), out int receiver_id)) //private message
			{
				//cant send private message to yourself
				if (player.User_ID != receiver_id)
				{
					await Websocket.SendPrivateChatMessageToPlayer(player, receiver_id, chatMessage);
				}
			}
			else
			{
				await Websocket.SendNotificationToPlayer(player, $"Unexpected value received for 'receiver'.", NotificationType.TOAST, ToastType.ERROR);
			}
		}

		public static async Task SendMessageToGroup(Player player, Group group, string chatMessage)
		{
			foreach (var member in group.Members)
			{
				await Websocket.SendChatMessageToPlayer(player, member.User_ID, chatMessage, MessageType.GROUP);
			}
		}

		public static async Task SendMessageGlobally(Player player, string chatMessage)
		{
			foreach (var userEntry in SharedData.Players) 
			{
				Player user = userEntry.Value;

				bool isInGroup = SharedData.GetGroupForPlayer(user) != null;

				if (!isInGroup)
				{
					await Websocket.SendChatMessageToPlayer(player, user.User_ID, chatMessage, MessageType.GLOBAL);
				}
			}
		}
	}
}