using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
	public interface IGameLogic
	{
		event Func<Player, string, NotificationType, ToastType?, Task> OnNotification;
		event Func<Group, string, NotificationType, ToastType?, Task> OnGroupNotification;
		event Func<Group, GameModel, Task>? OnGameInfoToGroup;
		event Func<Player, GameModel, Task>? OnGameInfoToPlayer;

		Task HandleGameAction(Player player, dynamic message);
		Task StartBetting(Group group);
		Task StartGame(Group group);
	}
}
