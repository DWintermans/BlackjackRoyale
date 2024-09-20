namespace BlackjackService
{
	public class Group
	{
		public static async Task HandleGroupAction(dynamic message, int user_id)
		{
			switch (message.action.ToString())
			{
				case "create_group":
					await CreateGroup(user_id);
					break;

				case "join_group":
					await JoinGroup(message.group_id.ToString(), user_id);
					break;

				case "leave_group":
					await LeaveGroup(user_id);
					break;

				default:
					await Websocket.ReturnMessageToUserID(user_id, "Unknown group action");
					break;
			}
		}

		public static async Task CreateGroup(int user_id)
		{
			//leavy current group if possible
			await LeaveGroup(user_id);

			string group_id = GenerateRandomGroupID();

			while (SharedData.groupMembers.ContainsKey(group_id))
			{
				group_id = GenerateRandomGroupID();
			}

			SharedData.groupMembers[group_id] = new List<int> { user_id };

			foreach (var group in SharedData.groupMembers)
			{
				Console.WriteLine("Group_ID: " + group.Key + " | User_IDs: " + string.Join(", ", group.Value));
			}

			Websocket.ReturnMessageToUserID(user_id, "Group with ID '" + group_id + "' created.");
			Websocket.ReturnMessageToUserID(user_id, $"You have joined group '{group_id}'.");
		}

		private static async Task JoinGroup(string group_id, int user_id)
		{
			//leavy current group if possible, cant leave group youre trying to join.
			if (!SharedData.groupMembers[group_id].Contains(user_id))
			{
				await LeaveGroup(user_id);
			}

			if (SharedData.groupMembers.ContainsKey(group_id))
			{
				if (!SharedData.groupMembers[group_id].Contains(user_id))
				{
					SharedData.groupMembers[group_id].Add(user_id);
					Console.WriteLine($"User {user_id} joined group {group_id}");

					Websocket.ReturnMessageToUserID(user_id, $"You have joined group '{group_id}'.");
				}
				else
				{
					Websocket.ReturnMessageToUserID(user_id, $"You are already a member of group '{group_id}'.");
				}

				foreach (var group in SharedData.groupMembers)
				{
					Console.WriteLine("Group_ID: " + group.Key + " | User_IDs: " + string.Join(", ", group.Value));
				}
			}
			else
			{
				Websocket.ReturnMessageToUserID(user_id, "Group does not exist.");
			}
		}

		private static async Task LeaveGroup(int user_id)
		{
			foreach (var group_id in SharedData.groupMembers.Keys.ToList())
			{
				if (SharedData.groupMembers[group_id].Contains(user_id))
				{
					SharedData.groupMembers[group_id].Remove(user_id);

					Console.WriteLine($"User {user_id} left group {group_id}");

					Websocket.ReturnMessageToUserID(user_id, $"You have left group '{group_id}'.");

					if (SharedData.groupMembers[group_id].Count == 0)
					{
						SharedData.groupMembers.Remove(group_id);
						Console.WriteLine($"Group {group_id} has been removed as it is empty.");
					}
				}
			}
		}

		private static string GenerateRandomGroupID()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Random random = new Random();

			return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
		}

	}
}
