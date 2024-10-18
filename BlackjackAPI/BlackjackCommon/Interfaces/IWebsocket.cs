using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using System.Net.WebSockets;

namespace BlackjackCommon.Interfaces
{
	public interface IWebsocket
	{
		Task Run();
		Task SendNotificationToSocket(WebSocket socket, string message);
		Task SendNotificationToPlayer(Player player, string message, NotificationType type, ToastType? toasttype = null);
		Task SendNotificationToGroup(Group group, string message, NotificationType type, ToastType? toasttype = null);
		Task SendPrivateChatMessageToPlayer(Player player, int receiver_id, string message);
		Task SendChatMessageToPlayer(Player player, int receiver_id, string message, MessageType type);
		Task SendGameInfoToGroup(Group group, GameModel gameModel);
		Task SendGameInfoToPlayer(Player player, GameModel gameModel);
		Task SendGroupInfoToPlayer(Player player, GroupModel model);
		Task SendLobbyInfoToPlayer(Player player, LobbyModel lobbyModel);
	}
}
