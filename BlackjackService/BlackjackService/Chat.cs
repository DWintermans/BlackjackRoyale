namespace BlackjackService
{
	public class Chat
	{
		public static async Task HandleChatAction(dynamic message, int user_id)
		{
			switch (message.action.ToString())
			{
				case "send_message":
					await SendMessage(user_id, message.receiver.ToString(), message.message.ToString());
					break;

				case "delete_message":
					//moderator only
					//await DeleteMessage();
					break;
				default:
					await Websocket.SendNotificationToUserID(user_id, "Unknown group action");
					break;
			}
		}

		public static async Task SendMessage(int sender_id, string receiver, string chatMessage)
		{
			if (receiver.ToString().ToUpper() == "GLOBAL") //global message
			{
				if (IsUserInGroup(sender_id))
				{
					await Websocket.SendNotificationToUserID(sender_id, $"As a member of this group, you can only send messages to this group.");
				}
				else
				{
					await SendMessageGlobally(sender_id, chatMessage);
				}
			}
			else if (receiver.ToString().ToUpper() == "GROUP") //group message
			{
				string group_id = null;

				//get group_id from participating sender
				foreach (var group in SharedData.groupMembers)
				{
					if (group.Value.Contains(sender_id))
					{
						group_id = group.Key;
						break;
					}
				}

				if (group_id != null)
				{
					await SendMessageToGroup(sender_id, group_id, chatMessage);
				}
				else
				{
					await Websocket.SendNotificationToUserID(sender_id, $"You are not a member of a group.");
				}
			}
			else if (int.TryParse(receiver.ToString(), out int receiver_id)) //private message
			{
				//cant send private message to yourself
				if (sender_id != receiver_id)
				{
					await Websocket.SendPrivateChatMessageToUserID(sender_id, receiver_id, chatMessage);
				}
			}
			else
			{
				await Websocket.SendNotificationToUserID(sender_id, $"Unexpected value received for 'receiver'.");
			}
		}


		public static async Task SendMessageToGroup(int sender_id, string group_id, string chatMessage)
		{
			foreach (int member_id in SharedData.groupMembers[group_id])
			{
				await Websocket.SendChatMessageToUserID(sender_id, member_id, chatMessage);
			}
		}

		public static async Task SendMessageGlobally(int sender_id, string chatMessage)
		{
			foreach (var user in SharedData.userIDToCliendIdMap)
			{
				int active_user_id = int.Parse(user.Key);

				bool isInGroup = false;
				foreach (var group in SharedData.groupMembers)
				{
					if (group.Value.Contains(active_user_id))
					{
						isInGroup = true;
						break;
					}
				}

				if (!isInGroup)
				{
					await Websocket.SendChatMessageToUserID(sender_id, active_user_id, chatMessage);
				}
			}
		}

		private static bool IsUserInGroup(int user_id)
		{
			foreach (var group in SharedData.groupMembers)
			{
				if (group.Value.Contains(user_id))
				{
					return true;
				}
			}
			return false;
		}


	}
}