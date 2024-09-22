namespace BlackjackService
{
	public class Group
	{
		private static Dictionary<int, bool> userReadyStatus = new Dictionary<int, bool>();

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

				case "ready":
					await Ready(user_id);
					break;

				case "unready":
					await Unready(user_id);
					break;

				default:
					await Websocket.SendNotificationToUserID(user_id, "Unknown group action");
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

			Websocket.SendNotificationToUserID(user_id, "Group with ID '" + group_id + "' created.");
			Websocket.SendNotificationToUserID(user_id, $"You have joined group '{group_id}'.");
		}

		private static async Task JoinGroup(string group_id, int user_id)
		{
			//check if group exists
			if (!SharedData.groupMembers.ContainsKey(group_id))
			{
				Websocket.SendNotificationToUserID(user_id, "Group does not exist.");
			} 
			else 
			{ 
				//check if already joined an existing group
				if (!SharedData.groupMembers[group_id].Contains(user_id))
				{
					//leavy current group if possible
					await LeaveGroup(user_id);
				}

				//cant join a full group
				if (SharedData.groupMembers.TryGetValue(group_id, out List<int> groupMembers) && groupMembers.Count >= 4)
				{
					await Websocket.SendNotificationToUserID(user_id, $"Group '{group_id}' is full!");
					return;
				}

				//add player to waitingroom if game has already started (if group already has deck of cards), add to playgroup on round start
				if (!SharedData.groupMembers[group_id].Contains(user_id) && 
					SharedData.groupDeck.ContainsKey(group_id) && 
					SharedData.groupDeck[group_id].Count > 0 &&
					SharedData.groupMembers.TryGetValue(group_id, out List<int> Members) && (Members.Count + 1) <= 4
					)
				{
					await AddPlayerToWaitingRoom(group_id, user_id);
					return;
				}

				//game has not started, try add to group if not already part of group.
				if (!SharedData.groupMembers[group_id].Contains(user_id) && !SharedData.groupDeck.ContainsKey(group_id) && SharedData.groupDeck[group_id].Count <= 0)
				{
					SharedData.groupMembers[group_id].Add(user_id);
					Console.WriteLine($"User {user_id} joined group {group_id}");

					Websocket.SendNotificationToUserID(user_id, $"You have joined group '{group_id}'.");
					Websocket.SendNotificationToGroupID(group_id, $"{user_id} joined the group.");
				}
				else
				{
					Websocket.SendNotificationToUserID(user_id, $"You are already a member of group '{group_id}'.");
				}

				foreach (var group in SharedData.groupMembers)
				{
					Console.WriteLine("Group_ID: " + group.Key + " | User_IDs: " + string.Join(", ", group.Value));
				}
			}
		}

		private static async Task LeaveGroup(int user_id)
		{
			//clear playerhands if possible
			if (SharedData.playerHands.ContainsKey(user_id))
			{
				SharedData.playerHands[user_id].Clear();
			}

			foreach (var group_id in SharedData.groupMembers.Keys.ToList())
			{
				if (SharedData.groupMembers[group_id].Contains(user_id))
				{
					SharedData.groupMembers[group_id].Remove(user_id);

					Console.WriteLine($"User {user_id} left group {group_id}");

					//set ready status to false
					userReadyStatus[user_id] = false;
					
					Websocket.SendNotificationToUserID(user_id, $"You have left group '{group_id}'.");
					Websocket.SendNotificationToGroupID(group_id, $"{user_id} left the group.");

					if (SharedData.groupMembers[group_id].Count == 0)
					{
						SharedData.groupMembers.Remove(group_id);
						Console.WriteLine($"Group {group_id} has been removed as it is empty.");
					}
				}
			}
		}

		private static async Task AddPlayerToWaitingRoom(string group_id, int user_id)
		{
			//create waiting room
			if (!SharedData.waitingRoom.ContainsKey(group_id))
			{
				SharedData.waitingRoom[group_id] = new List<int>();
			}

			SharedData.waitingRoom[group_id].Add(user_id);
			await Websocket.SendNotificationToUserID(user_id, "You have joined the waiting room. You will enter the game when the current round is over.");
		}

		public static async Task MovePlayersFromWaitingRoom(string group_id)
		{
			if (SharedData.waitingRoom.ContainsKey(group_id))
			{
				List<int> newPlayers = SharedData.waitingRoom[group_id];
				foreach (int user_id in newPlayers)
				{
					SharedData.groupMembers[group_id].Add(user_id);
					await Websocket.SendNotificationToUserID(user_id, "The current round is over. You are now in the game.");
				}

				SharedData.waitingRoom[group_id].Clear();
			}
		}

		private static async Task Ready(int user_id)
		{
			await SetReadyStatus(user_id, true);

		}

		private static async Task Unready(int user_id)
		{
			await SetReadyStatus(user_id, false);
		}

		private static async Task SetReadyStatus(int user_id, bool isReady)
		{
			string group_id = GetGroupIDForUserID(user_id);

			if (group_id != null)
			{
				//if game has already started, dont allow ready/unready commands
				if (SharedData.groupDeck.ContainsKey(group_id) && SharedData.groupDeck[group_id].Count > 0)
				{
					await Websocket.SendNotificationToUserID(user_id, "The game has already started. You cannot change your ready status now.");
					return;
				}

				userReadyStatus[user_id] = isReady;

				string message = isReady ? "You are now ready." : "You are now unready.";
				await Websocket.SendNotificationToUserID(user_id, message);

				await CheckVotesAndStartGame(group_id);
			}
			else
			{
				await Websocket.SendNotificationToUserID(user_id, "You must be part of a group to set your readiness.");
			}
		}

		//check if majority is ready to play
		private static async Task CheckVotesAndStartGame(string group_id)
		{
			if (SharedData.groupMembers.TryGetValue(group_id, out List<int> groupMembers))
			{
				//debug
				foreach (int user_id in groupMembers)
				{
					bool isReady = userReadyStatus.ContainsKey(user_id) && userReadyStatus[user_id];

					Console.WriteLine($"Group ID: {group_id} | User ID: {user_id} | Ready: {isReady}");
				}
				//enddebug

				int readyCount = groupMembers.Count(user_id => userReadyStatus.ContainsKey(user_id) && userReadyStatus[user_id]);
				int totalMembers = groupMembers.Count;

				await Websocket.SendNotificationToGroupID(group_id, $"{readyCount}/{totalMembers} players are ready.");

				if (readyCount > totalMembers / 2)
				{
					await Websocket.SendNotificationToGroupID(group_id, "The game is starting now!");
					await Game.StartGame(group_id);
				}
			}
		}

		private static string GetGroupIDForUserID(int user_id)
		{
			foreach (var group in SharedData.groupMembers)
			{
				if (group.Value.Contains(user_id))
				{
					return group.Key;
				}
			}
			return null;
		}

		private static string GenerateRandomGroupID()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Random random = new Random();

			return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
		}

	}
}
