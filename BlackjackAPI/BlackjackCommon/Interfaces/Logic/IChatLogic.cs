using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Logic
{
    public interface IChatLogic
    {
        event Func<Player, string, NotificationType, ToastType?, Task> OnNotification;
        event Func<Player, int, string, MessageType, Task> OnMessage;
        event Func<Player, int, string, Task> OnPrivateMessage;

        Response<List<MessageListModel>> RetrieveMessageList(int user_id);
        Response<List<MessageListModel>> RetrievePrivateMessages(int user_id, int other_user_id);
        void SaveChatMessage(int user_id, int receiver_id, string? group_id, string message);

        Task HandleChatAction(Player player, dynamic message);
    }
}
