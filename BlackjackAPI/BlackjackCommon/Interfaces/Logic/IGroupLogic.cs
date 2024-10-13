using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IGroupLogic
	{
		event Func<Player, string, NotificationType, ToastType?, Task>? OnNotification;
		event Func<Group, string, NotificationType, ToastType?, Task>? OnGroupNotification;
		event Func<Player, GroupModel, Task>? OnGroupInfoToPlayer;
		event Func<Player, LobbyModel, Task>? OnLobbyInfoToPlayer;


		Task HandleGroupAction(Player player, dynamic message);
		Task MovePlayersFromWaitingRoom(Group group);
	}
}