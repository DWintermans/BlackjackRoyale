using BlackjackCommon.Data.SharedData;
using BlackjackCommon.Interfaces.Logic;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System.Net.WebSockets;
using Group = BlackjackCommon.Models.Group;

namespace BlackjackLogic
{
	public class GroupLogic : IGroupLogic
	{
		public event Func<Player, string, NotificationType, ToastType?, Task>? OnNotification;
		public event Func<Group, string, NotificationType, ToastType?, Task>? OnGroupNotification;
		public event Func<Player, GroupModel, Task>? OnGroupInfoToPlayer;
		public event Func<Player, LobbyModel, Task>? OnLobbyInfoToPlayer;

		private const int MaxGroupSize = 4;

		private readonly IPlayerLogic _playerLogic;
		private readonly IGameLogic _gameLogic;


		public GroupLogic(IPlayerLogic playerLogic, IGameLogic gameLogic)
		{
			_playerLogic = playerLogic;
			_gameLogic = gameLogic;
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
					await OnNotification?.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
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
						isReady: member.IsReady,
						credits: null //add per member
					);

					model.Members.Add(memberModel);
				}

				foreach (Player member in group.Members)
				{
					var specificMemberModel = new GroupModel
					{
						Group_ID = model.Group_ID,
						Members = new List<Member>()
					};

					foreach (Member m in model.Members)
					{
						var specificMemberInfo = new Member(
							user_id: m.User_ID,
							name: m.Name,
							inWaitingRoom: m.InWaitingRoom,
							isReady: m.IsReady,
							credits: member.User_ID == m.User_ID ? member.Credits : (int?)null
						);

						specificMemberModel.Members.Add(specificMemberInfo);
					}

					await OnGroupInfoToPlayer?.Invoke(member, specificMemberModel);
				}
			}
			else
			{
				GroupModel model = new GroupModel
				{
					Group_ID = null,
					Members = null
				};

				await OnGroupInfoToPlayer?.Invoke(player, model);
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
					await OnLobbyInfoToPlayer?.Invoke(player, lobbyModel);
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

			await OnLobbyInfoToPlayer?.Invoke(player, lobbyModel);
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
						isReady: member.IsReady,
						credits: member.User_ID == player.User_ID ? member.Credits : (int?)null
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
			await OnGroupInfoToPlayer?.Invoke(player, model);
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

			//await OnNotification?.Invoke(player, $"Group with ID {group.Group_ID} created.", NotificationType.TOAST, ToastType.SUCCESS);
			await OnNotification?.Invoke(player, $"You have created group {group.Group_ID}. Press 'ready' to start betting.", NotificationType.TOAST, ToastType.INFO);
		}

		private async Task LeaveGroup(Player player)
		{
			//save group to send groupinfo update to
			Group group = SharedData.GetGroupForPlayer(player);

			//if player left after placing a bet: possibility of losing all credits
			if (player.Credits < 10)
			{
				player.Credits = 100;
			}

			//leave group
			if (group != null)
			{
				group.Members.RemoveAll(p => p.User_ID == player.User_ID);

				player.Hands.Clear(); 
				player.IsReady = false;

				await OnNotification?.Invoke(player, $"You have left group '{group.Group_ID}'.", NotificationType.TOAST, ToastType.INFO);
				
				await OnGroupNotification?.Invoke(group, $"{player.Name} left the group.", NotificationType.GROUP, default);

				Console.WriteLine($"User {player.User_ID} left group {group.Group_ID}");

				if (group.Members.Count == 0)
				{
					if (group.WaitingRoom.Count > 0)
					{
						foreach (var member in group.WaitingRoom.ToList())
						{
							await OnNotification?.Invoke(member, "The group you were in the waitingroom for no longer exists.", NotificationType.TOAST, ToastType.INFO);
						}
					}

					SharedData.Groups.Remove(group.Group_ID);
					Console.WriteLine($"Group {group.Group_ID} has been removed as it is empty.");
				}

				await CheckGroup(player);

				//send updated groupinfo to old group that player left, if group exists
				foreach (Player member in group.Members)
				{
					await ForceCheckGroup(member);
				}

				//left while betting? start game if possible
				if (group.Members.Count != 0) 
				{
					if (group.Status == Group.GroupStatus.BETTING && group.Bets.Count == group.Members.Count)
					{
						group.Status = Group.GroupStatus.PLAYING;
						await _gameLogic.StartGame(group);
					}
				}
			}

			//if in waiting room
			Group waitingRoomGroup = SharedData.GetGroupForWaitingroomPlayer(player);
			if (waitingRoomGroup != null) 
			{
				await OnNotification?.Invoke(player, $"You have left the waitingroom for group '{waitingRoomGroup.Group_ID}'.", NotificationType.TOAST, ToastType.INFO);

				await OnGroupNotification?.Invoke(waitingRoomGroup, $"{player.Name} left the waitingroom.", NotificationType.GROUP, default);
				
				Console.WriteLine($"User {player.User_ID} left group {waitingRoomGroup.Group_ID} waitingroom.");

				waitingRoomGroup.WaitingRoom.Remove(player);
				Console.WriteLine(waitingRoomGroup);
			}
		}

		private async Task JoinGroup(Player player, string group_id)
		{
			//check if group exists
			if (!SharedData.Groups.ContainsKey(group_id))
			{
				await OnNotification?.Invoke(player, "Group does not exist.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			Group group = SharedData.Groups[group_id];

			//check if already in group
			if (group.Members.Any(p => p.User_ID == player.User_ID))
			{
				await OnNotification?.Invoke(player, "You are already in this group.", NotificationType.TOAST, ToastType.INFO);
				return;
			}

			//cant join a full group
			if (group.Members.Count >= MaxGroupSize)
			{
				await OnNotification?.Invoke(player, $"Group '{group_id}' is full!", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			//leave current group is possible
			Group oldGroup = SharedData.GetGroupForPlayer(player);
			Group oldWaitingroomGroup = SharedData.GetGroupForWaitingroomPlayer(player);
			if ((oldGroup != null && oldGroup.Group_ID != group.Group_ID) || (oldWaitingroomGroup != null && oldWaitingroomGroup.Group_ID != group.Group_ID))
			{
				LeaveGroup(player);
			}

			//if game has started -> add to waitingroom 
			if ((group.Status == Group.GroupStatus.BETTING || group.Status == Group.GroupStatus.PLAYING) && (group.Members.Count + group.WaitingRoom.Count < MaxGroupSize))
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
			OnNotification?.Invoke(player, $"You have joined group '{group_id}'.", NotificationType.TOAST, ToastType.INFO);
			OnGroupNotification?.Invoke(group, $"{player.Name} joined the group.", NotificationType.GROUP, default);

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

				await OnNotification?.Invoke(player, "You have joined the waiting room. You will enter the game when the current round is over.", NotificationType.TOAST, ToastType.INFO);
				await OnGroupNotification?.Invoke(group, $"{player.Name} is waiting and will join next round.", NotificationType.GROUP, default);
			}
		}

		public async Task MovePlayersFromWaitingRoom(Group group)
		{
			if (group.WaitingRoom.Count > 0)
			{
				foreach (var player in group.WaitingRoom.ToList())
				{
					group.Members.Add(player);
					await ForceCheckGroup(player);

					await OnNotification?.Invoke(player, "The current round is over. You are now in the game.", NotificationType.TOAST, ToastType.INFO);
					await OnGroupNotification?.Invoke(group, $"{player.Name} joined the group.", NotificationType.GROUP, default);
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
				await OnNotification?.Invoke(player, "You must be part of a group to set your readiness.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			//if game has already started, dont allow ready/unready commands
			if (group.Status != Group.GroupStatus.WAITING)
			{
				await OnNotification?.Invoke(player, "The game has already started. You cannot change your ready status now.", NotificationType.TOAST, ToastType.WARNING);
				return;
			}

			player.IsReady = isReady;
			await OnNotification?.Invoke(player, isReady ? "You are now ready." : "You are now unready.", NotificationType.TOAST, ToastType.INFO);

			await CheckVotesAndStartBetting(group);
		}

		//check if majority is ready to play
		private async Task CheckVotesAndStartBetting(Group group)
		{
			//debug
			foreach (var player in group.Members)
			{
				Console.WriteLine($"Group ID: {group.Group_ID} | User ID: {player.User_ID} | Ready: {player.IsReady}");
			}
			//enddebug

			int readyCount = group.Members.Count(player => player.IsReady);
			int totalMembers = group.Members.Count;

			await OnGroupNotification?.Invoke(group, $"{readyCount}/{totalMembers} players are ready.", NotificationType.GROUP, default);

			if (readyCount > totalMembers / 2)
			{
				group.Status = Group.GroupStatus.BETTING;
				await _gameLogic.StartBetting(group);
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
