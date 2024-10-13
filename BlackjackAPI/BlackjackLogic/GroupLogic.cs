using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Data.SharedData;
using BlackjackCommon.ViewModels;
using BlackjackCommon.Interfaces;
using System.Text.RegularExpressions;
using BlackjackCommon.Models;
using Group = BlackjackCommon.Models.Group;


namespace BlackjackLogic
{
	public class GroupLogic : IGroupLogic
	{
		private readonly IWebsocket _websocket;
		private const int MaxGroupSize = 4;

		public GroupLogic(IWebsocket websocket)
		{
			_websocket = websocket;
		}

		public async Task HandleGroupAction(Player player, dynamic message)
		{
			switch (message.action.ToString())
			{
				case "create_group":
					await CreateGroup(player);
					await ForceShowLobby();
					await ForceCheckGroup(player);
					break;

				case "join_group":
					await JoinGroup(player, message.group_id.ToString());
					await ForceShowLobby();
					await ForceCheckGroup(player);
					break;

				case "leave_group":
					LeaveGroup(player);
					await ForceShowLobby();
					break;

				case "ready":
					await Ready(player);
					await ForceCheckGroup(player);
					break;

				case "unready":
					await Unready(player);
					await ForceCheckGroup(player);
					break;

				case "check_group":
					await CheckGroup(player);
					break;

				case "show_lobby":
					await ShowLobby(player);
					break;

				default:
					await _websocket.SendNotificationToPlayer(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
					return;
			}
		}

		private async Task ForceCheckGroup(Player player)
		{
            Group group = SharedData.GetGroupForPlayer(player);

			if (group != null)
			{
				GroupModel model = new GroupModel
				{
					Group_ID = group.Group_ID,
					Members = new List<Member>()
				};

				foreach (Player member in group.Members)
				{
					var memberModel = new Member(
						user_id: member.User_ID,
						name: member.Name,
						inWaitingRoom: group.WaitingRoom.Contains(member),
						isReady: member.IsReady
					);

					model.Members.Add(memberModel);
				}

				foreach (Player member in group.Members)
				{
					await _websocket.SendGroupInfoToPlayer(member, model);
				}
			}
			else
			{
				GroupModel model = new GroupModel
				{
					Group_ID = null,
					Members = null
				};

				await _websocket.SendGroupInfoToPlayer(player, model);
			}
		}

		private async Task ForceShowLobby()
		{
			LobbyModel lobbyModel = new LobbyModel
			{
				Type = "LOBBY",
				Lobby = new List<Lobby>()
			};

			foreach (var groupEntry in SharedData.Groups)
			{
                BlackjackCommon.Models.Group currentGroup = groupEntry.Value;

				int memberCount = currentGroup.Members.Count;
				int waitingRoomCount = currentGroup.WaitingRoom.Count;

				Lobby lobby = new Lobby
				{
					Group_ID = currentGroup.Group_ID,
					Members = memberCount + waitingRoomCount
				};

				lobbyModel.Lobby.Add(lobby);
			}

			foreach (var playerEntry in SharedData.Players)
			{
				Player player = playerEntry.Value;

				Group group = SharedData.GetGroupForPlayer(player);
				if (group == null)
				{
					await _websocket.SendLobbyInfoToPlayer(player, lobbyModel);
				}
			}
		}

		private async Task ShowLobby(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			//player in group? dont show lobby
			if (group != null)
			{
				return;
			}

			LobbyModel lobbyModel = new LobbyModel
			{
				Type = "LOBBY",
				Lobby = new List<Lobby>()
			};

			foreach (var groupEntry in SharedData.Groups)
			{
				Group currentGroup = groupEntry.Value;

				int memberCount = currentGroup.Members.Count;
				int waitingRoomCount = currentGroup.WaitingRoom.Count;

				Lobby lobby = new Lobby
				{
					Group_ID = currentGroup.Group_ID,
					Members = memberCount + waitingRoomCount
				};

				lobbyModel.Lobby.Add(lobby);
			}

			await _websocket.SendLobbyInfoToPlayer(player, lobbyModel);
		}

		private async Task CheckGroup(Player player)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			GroupModel model;

			if (group != null)
			{
				model = new GroupModel
				{
					Group_ID = group.Group_ID,
					Members = new List<Member>()
				};

				foreach (Player member in group.Members)
				{
					var memberModel = new Member(
						user_id: member.User_ID,
						name: member.Name,
						inWaitingRoom: group.WaitingRoom.Contains(member),
						isReady: member.IsReady
					);

					model.Members.Add(memberModel);
				}
			}
			else
			{
				model = new GroupModel
				{
					Group_ID = null,
					Members = null
				};
			}
			await _websocket.SendGroupInfoToPlayer(player, model);
		}

		private async Task CreateGroup(Player player)
		{
			//leave current group if possible
			LeaveGroup(player);

			string group_id = GenerateRandomGroupID();
			while (SharedData.Groups.ContainsKey(group_id))
			{
				group_id = GenerateRandomGroupID();
			}

			//create unique group id for database
			string unique_id = $"{group_id}_{Guid.NewGuid().ToString()}";

			Group group = new Group(group_id, unique_id);

			//add to group
			group.Members.Add(player);

			//add group to list
			SharedData.Groups[group_id] = group;

			foreach (var g in SharedData.Groups)
			{
				Console.WriteLine(group.Unique_Group_ID);
				Console.WriteLine("Group_ID: " + g.Key + " | User_IDs: " + string.Join(", ", g.Value.Members.Select(m => m.User_ID)));
			}

			await _websocket.SendNotificationToPlayer(player, $"Group with ID {group.Group_ID} created.", NotificationType.TOAST, ToastType.SUCCESS);
			await _websocket.SendNotificationToPlayer(player, $"You have joined group {group.Group_ID}.", NotificationType.TOAST, ToastType.INFO);
		}

		private async Task LeaveGroup(Player player)
		{
			//save group to send groupinfo update to
			Group group = SharedData.GetGroupForPlayer(player);

			//leave group
			if (group != null)
			{
				foreach (var groups in SharedData.Groups.Values.ToList())
				{
					if (groups.Members.Any(p => p.User_ID == player.User_ID))
					{
						groups.Members.RemoveAll(p => p.User_ID == player.User_ID);

						player.ClearHand();
						player.IsReady = false;

						await _websocket.SendNotificationToPlayer(player, $"You have left group '{groups.Group_ID}'.", NotificationType.TOAST, ToastType.INFO);
						await _websocket.SendNotificationToGroup(groups, $"{player.Name} left the group.", NotificationType.GROUP);

						Console.WriteLine($"User {player.User_ID} left group {groups.Group_ID}");

						if (group.Members.Count == 0)
						{
							SharedData.Groups.Remove(groups.Group_ID);
							Console.WriteLine($"Group {groups.Group_ID} has been removed as it is empty.");
						}

						break;
					}
				}

				await CheckGroup(player);

				//send updated groupinfo to old group that player left, if group exists
				foreach (Player member in group.Members)
				{
					await ForceCheckGroup(member);
				}
			}
		}

		private async Task JoinGroup(Player player, string group_id)
		{
			//check if group exists
			if (!SharedData.Groups.ContainsKey(group_id))
			{
				await _websocket.SendNotificationToPlayer(player, "Group does not exist.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			Group group = SharedData.Groups[group_id];

			//check if already in group
			if (group.Members.Any(p => p.User_ID == player.User_ID))
			{
				await _websocket.SendNotificationToPlayer(player, "You are already in this group.", NotificationType.TOAST, ToastType.INFO);
				return;
			}

			//cant join a full group
			if (group.Members.Count >= MaxGroupSize)
			{
				await _websocket.SendNotificationToPlayer(player, $"Group '{group_id}' is full!", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			//leave current group is possible
			Group oldGroup = SharedData.GetGroupForPlayer(player);
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
			_websocket.SendNotificationToPlayer(player, $"You have joined group '{group_id}'.", NotificationType.TOAST, ToastType.INFO);
			_websocket.SendNotificationToGroup(group, $"{player.Name} joined the group.", NotificationType.GROUP);

			foreach (var grp in SharedData.Groups)
			{
				Console.WriteLine("Group_ID: " + grp.Key + " | User_IDs: " + string.Join(", ", grp.Value.Members.Select(p => p.User_ID)));
			}

		}

		private async Task AddPlayerToWaitingRoom(Group group, Player player)
		{
			if (!group.WaitingRoom.Contains(player))
			{
				group.WaitingRoom.Add(player);

				await _websocket.SendNotificationToPlayer(player, "You have joined the waiting room. You will enter the game when the current round is over.", NotificationType.TOAST, ToastType.INFO);
				await _websocket.SendNotificationToGroup(group, $"{player.Name} is waiting and will join next round.", NotificationType.GROUP);
			}
		}

		public async Task MovePlayersFromWaitingRoom(Group group)
		{
			if (group.WaitingRoom.Count > 0)
			{
				foreach (var player in group.WaitingRoom.ToList())
				{
					group.Members.Add(player);

					await _websocket.SendNotificationToPlayer(player, "The current round is over. You are now in the game.", NotificationType.TOAST, ToastType.INFO);
				}
				group.WaitingRoom.Clear();
			}
		}

		private async Task Ready(Player player)
		{
			await SetReadyStatus(player, true);
		}

		private async Task Unready(Player player)
		{
			await SetReadyStatus(player, false);
		}

		private async Task SetReadyStatus(Player player, bool isReady)
		{
			Group group = SharedData.GetGroupForPlayer(player);

			if (group == null)
			{
				await _websocket.SendNotificationToPlayer(player, "You must be part of a group to set your readiness.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			//if game has already started, dont allow ready/unready commands
			if (group.Deck.Count > 0)
			{
				await _websocket.SendNotificationToPlayer(player, "The game has already started. You cannot change your ready status now.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			player.SetReadyStatus(isReady);
			await _websocket.SendNotificationToPlayer(player, isReady ? "You are now ready." : "You are now unready.", NotificationType.TOAST, ToastType.INFO);

			await CheckVotesAndStartGame(group);
		}

		//check if majority is ready to play
		private async Task CheckVotesAndStartGame(Group group)
		{
			//debug
			foreach (var player in group.Members)
			{
				Console.WriteLine($"Group ID: {group.Group_ID} | User ID: {player.User_ID} | Ready: {player.IsReady}");
			}
			//enddebug

			int readyCount = group.Members.Count(player => player.IsReady);
			int totalMembers = group.Members.Count;

			await _websocket.SendNotificationToGroup(group, $"{readyCount}/{totalMembers} players are ready.", NotificationType.GROUP);

			if (readyCount > totalMembers / 2)
			{
				await _websocket.SendNotificationToGroup(group, "The game is starting now!", NotificationType.GROUP);
				await _gameLogic.StartGame(group);
			}
		}

		private string GenerateRandomGroupID()
		{
			const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
			Random random = new Random();

			return new string(Enumerable.Repeat(chars, 6).Select(s => s[random.Next(s.Length)]).ToArray());
		}
	}
}
