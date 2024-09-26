namespace BlackjackService
{
	public class Group
	{
		private const int MaxGroupSize = 4;

		public string Group_ID { get; private set; }
		public List<Player> Members { get; private set; }
		public List<string> Deck { get; private set; }
		public List<Player> WaitingRoom { get; private set; }

		public Group(string group_id)
		{
			Group_ID = group_id;
			Members = new List<Player>();
			Deck = new List<string>();
			WaitingRoom = new List<Player>();
		}

		public static async Task HandleGroupAction(Player player, dynamic message)
		{
			switch (message.action.ToString())
			{
				case "create_group":
					await CreateGroup(player);
					break;

				case "join_group":
					await JoinGroup(player, message.group_id.ToString());
					break;

				case "leave_group":
					LeaveGroup(player);
					break;

				case "ready":
					await Ready(player);
					break;

				case "unready":
					await Unready(player);
					break;

				default:
					await Websocket.SendNotificationToPlayer(player, "Unknown group action");
					break;
			}
		}

		private static async Task CreateGroup(Player player)
		{
			//leave current group if possible
			LeaveGroup(player);

			string group_id = GenerateRandomGroupID();
			while (SharedData.Groups.ContainsKey(group_id))
			{
				group_id = GenerateRandomGroupID();
			}

			Group group = new Group(group_id);

			//add to group
			group.Members.Add(player);

			//add group to list
			SharedData.Groups[group_id] = group;

			foreach (var g in SharedData.Groups)
			{
				Console.WriteLine("Group_ID: " + g.Key + " | User_IDs: " + string.Join(", ", g.Value.Members.Select(m => m.User_ID)));
			}

			await Websocket.SendNotificationToPlayer(player, $"Group with ID {group.Group_ID} created.");
			await Websocket.SendNotificationToPlayer(player, $"You have joined group {group.Group_ID}.");
		}

		private static void LeaveGroup(Player player)
		{
			foreach (var group in SharedData.Groups.Values.ToList())
			{
				if (group.Members.Any(p => p.User_ID == player.User_ID))
				{
					group.Members.RemoveAll(p => p.User_ID == player.User_ID);

					player.ClearHand();
					player.IsReady = false;

					Websocket.SendNotificationToPlayer(player, $"You have left group '{group.Group_ID}'.");
					Websocket.SendNotificationToGroup(group, $"{player.User_ID} left the group.");

					Console.WriteLine($"User {player.User_ID} left group {group.Group_ID}");

					if (group.Members.Count == 0)
					{
						SharedData.Groups.Remove(group.Group_ID);
						Console.WriteLine($"Group {group.Group_ID} has been removed as it is empty.");
					}

					break;
				}
			}
		}

		private static async Task JoinGroup(Player player, string group_id)
		{
			//check if group exists
			if (!SharedData.Groups.ContainsKey(group_id))
			{
				await Websocket.SendNotificationToPlayer(player, "Group does not exist.");
				return;
			}

			Group group = SharedData.Groups[group_id];

			//check if already in group
			if (group.Members.Any(p => p.User_ID == player.User_ID))
			{
				await Websocket.SendNotificationToPlayer(player, "You are already in this group.");
				return;
			}

			//cant join a full group
			if (group.Members.Count >= MaxGroupSize)
			{
				await Websocket.SendNotificationToPlayer(player, $"Group '{group_id}' is full!");
				return;
			}

			//leave current group is possible
			Group oldGroup = GetGroupForPlayer(player);
			if (oldGroup != null && oldGroup.Group_ID != group.Group_ID)
			{
				LeaveGroup(player);
			}

			//game has started -> add to waitingroom 
			if (group.Deck.Count > 0 && (group.Members.Count + group.WaitingRoom.Count < MaxGroupSize))
			{
				await AddPlayerToWaitingRoom(group, player);
				return;
			}

			//game has not started? -> add to group
			if (group.Members.Count < MaxGroupSize && !group.Members.Contains(player))
			{
				group.Members.Add(player);
				player.IsReady = false;
			}

			Console.WriteLine($"User {player.User_ID} joined group {group_id}");
			Websocket.SendNotificationToPlayer(player, $"You have joined group '{group_id}'.");
			Websocket.SendNotificationToGroup(group, $"{player.User_ID} joined the group.");

			foreach (var grp in SharedData.Groups)
			{
				Console.WriteLine("Group_ID: " + grp.Key + " | User_IDs: " + string.Join(", ", grp.Value.Members.Select(p => p.User_ID)));
			}

		}

		private static async Task AddPlayerToWaitingRoom(Group group, Player player)
		{
			if (!group.WaitingRoom.Contains(player))
			{
				group.WaitingRoom.Add(player);

				await Websocket.SendNotificationToPlayer(player, "You have joined the waiting room. You will enter the game when the current round is over.");
				await Websocket.SendNotificationToGroup(group, $"{player.User_ID} is waiting and will join next round.");
			}
		}

		public static async Task MovePlayersFromWaitingRoom(Group group)
		{
			if (group.WaitingRoom.Count > 0)
			{
				foreach (var player in group.WaitingRoom.ToList())
				{
					group.Members.Add(player);

					await Websocket.SendNotificationToPlayer(player, "The current round is over. You are now in the game.");
				}
				group.WaitingRoom.Clear();
			}
		}

		private static async Task Ready(Player player)
		{
			await SetReadyStatus(player, true);
		}

		private static async Task Unready(Player player)
		{
			await SetReadyStatus(player, false);
		}

		private static async Task SetReadyStatus(Player player, bool isReady)
		{
			Group group = GetGroupForPlayer(player);

			if (group == null)
			{
				await Websocket.SendNotificationToPlayer(player, "You must be part of a group to set your readiness.");
				return;
			}

			//if game has already started, dont allow ready/unready commands
			if (group.Deck.Count > 0)
			{
				await Websocket.SendNotificationToPlayer(player, "The game has already started. You cannot change your ready status now.");
				return;
			}

			//set status like this to prevent instance issues.
			var groupPlayer = group.Members.FirstOrDefault(p => p.User_ID == player.User_ID);
			if (groupPlayer != null)
			{
				groupPlayer.SetReadyStatus(isReady);
				await Websocket.SendNotificationToPlayer(player, isReady ? "You are now ready." : "You are now unready.");
			}

			await CheckVotesAndStartGame(group);

		}

		//check if majority is ready to play
		private static async Task CheckVotesAndStartGame(Group group)
		{	
			//debug
			foreach (var player in group.Members)
			{
				Console.WriteLine($"Group ID: {group.Group_ID} | User ID: {player.User_ID} | Ready: {player.IsReady}");
			}
			//enddebug

			int readyCount = group.Members.Count(player => player.IsReady);
			int totalMembers = group.Members.Count;

			await Websocket.SendNotificationToGroup(group, $"{readyCount}/{totalMembers} players are ready.");

			if (readyCount > totalMembers / 2)
			{
				await Websocket.SendNotificationToGroup(group, "The game is starting now!");
				//await Game.StartGame(group);
			}			
		}

		private static Group GetGroupForPlayer(Player player)
		{
			foreach (var group in SharedData.Groups.Values)
			{
				if (group.Members.Any(p => p.User_ID == player.User_ID))
				{
					return group; 
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
