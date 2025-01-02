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

        void SaveEvent<T>(T model, string group_id, int round_number) where T : class;
        Task HandleGameAction(Player player, dynamic message);
        Task StartBetting(Group group);
        Task StartGame(Group group);
		void SavePlaytime(int user_ID, DateTime? joinedAt);
	}
}
