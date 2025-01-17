using BlackjackCommon.Data.SharedData;
using BlackjackLogic.Interfaces.Logic;
using BlackjackLogic.Interfaces.Repository;
using BlackjackCommon.Models;
using BlackjackCommon.ViewModels;
using Group = BlackjackCommon.Models.Group;
using Player = BlackjackCommon.Models.Player;

namespace BlackjackLogic
{
    public class ChatLogic : IChatLogic
    {
        private readonly IChatRepository _chatDAL;
        private readonly IFriendRepository _friendDAL;
        public ChatLogic(IChatRepository chatDAL, IFriendRepository friendDAL)
        {
            _chatDAL = chatDAL;
            _friendDAL = friendDAL;
        }

        private const int MaxGroupSize = 4;

        public event Func<Player, string, NotificationType, ToastType?, Task>? OnNotification;
        public event Func<Player, int, string, MessageType, Task>? OnMessage;
        public event Func<Player, int, string, Task>? OnPrivateMessage;

        public Response<List<MessageListModel>> RetrieveMessageList(int user_id)
        {
            try
            {
                var messages = _chatDAL.RetrieveMessageList(user_id);

                if (messages.Count == 0)
                {
                    return new Response<List<MessageListModel>>(null, "NoMessagesFound");
                }

                if (messages == null)
                {
                    return new Response<List<MessageListModel>>(null, "Default");
                }

                return new Response<List<MessageListModel>>(messages, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public Response<List<MessageListModel>> RetrievePrivateMessages(int user_id, int other_user_id)
        {
            try
            {
                var privateMessages = _chatDAL.RetrievePrivateMessages(user_id, other_user_id);

                if (privateMessages == null || privateMessages.Count == 0)
                {
                    return new Response<List<MessageListModel>>("NoMessagesFound");
                }

                return new Response<List<MessageListModel>>(privateMessages, "Success");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        public async Task HandleChatAction(Player player, dynamic message)
        {
            switch (message.action.ToString())
            {
                case "send_message":
                    await SendMessage(player, message.receiver.ToString(), message.message.ToString());
                    break;

                case "delete_message":
                    //moderator only
                    //await DeleteMessage();
                    break;
                default:
                    if (OnNotification != null)
                    {
                        await OnNotification.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
                    }
                    break;
            }
        }

        public async Task SendMessage(Player player, string receiver, string chatMessage)
        {
            Group? group = SharedData.GetGroupForPlayer(player);

            if (receiver.ToString().ToUpper() == "GLOBAL") //global message
            {
                if (group == null)
                {
                    await SendMessageGlobally(player, chatMessage);
                    SaveChatMessage(player.User_ID, 0, "GLOBAL", chatMessage);
                }
                else
                {
                    if (OnNotification != null)
                    {
                        await OnNotification.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
                    }
                }
            }
            else if (receiver.ToString().ToUpper() == "GROUP") //group message
            {
                if (group != null)
                {
                    await SendMessageToGroup(player, group, chatMessage);
                    SaveChatMessage(player.User_ID, 0, group.Unique_Group_ID, chatMessage);
                }
                else
                {
                    if (OnNotification != null)
                    {
                        await OnNotification.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
                    }
                }
            }
            else if (int.TryParse(receiver.ToString(), out int receiver_id)) //private message
            {
                //cant send private message to yourself
                if (player.User_ID != receiver_id)
                {
                    //only allow messages between players that are friends
                    if (_friendDAL.FriendshipExists(player.User_ID, receiver_id))
                    {
                        if (OnPrivateMessage != null)
                        {
                            await OnPrivateMessage.Invoke(player, receiver_id, chatMessage);
                        }

                        SaveChatMessage(player.User_ID, receiver_id, null, chatMessage);
                    }
                    else
                    {
                        if (OnNotification != null)
                        {
                            await OnNotification.Invoke(player, "You must be friends before sending a private message.", NotificationType.TOAST, ToastType.ERROR);
                        }
                    }
                }
            }
            else
            {
                if (OnNotification != null)
                {
                    await OnNotification.Invoke(player, "Unknown group action", NotificationType.TOAST, ToastType.ERROR);
                }
            }
        }

        public async Task SendMessageToGroup(Player player, Group? group, string chatMessage)
        {
            if (group != null)
            {
                foreach (var member in group.Members)
                {
                    if (OnMessage != null)
                    {
                        await OnMessage.Invoke(player, member.User_ID, chatMessage, MessageType.GROUP);
                    }
                }
            }
        }

        public async Task SendMessageGlobally(Player player, string chatMessage)
        {
            foreach (var userEntry in SharedData.Players)
            {
                Player user = userEntry.Value;

                bool isInGroup = SharedData.GetGroupForPlayer(user) != null;

                if (!isInGroup)
                {
                    if (OnMessage != null)
                    {
                        await OnMessage.Invoke(player, user.User_ID, chatMessage, MessageType.GLOBAL);
                    }
                }
            }
        }

        public void SaveChatMessage(int user_id, int receiver_id, string? group_id, string message)
        {
            if (user_id <= 0)
            {
                throw new ArgumentException("User ID must be provided.");
            }

            if (string.IsNullOrEmpty(message))
            {
                throw new ArgumentException("Message cannot be empty.");
            }

            if (receiver_id <= 0 && string.IsNullOrEmpty(group_id))
            {
                throw new ArgumentException("Either receiver_id or group_id must be provided.");
            }

            if (receiver_id > 0 && !string.IsNullOrEmpty(group_id))
            {
                throw new ArgumentException("Only one of receiver_id or group_id can be provided, not both.");
            }

            try
            {
                _chatDAL.SaveChatMessage(user_id, receiver_id, group_id ?? string.Empty, message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }

        }
    }
}