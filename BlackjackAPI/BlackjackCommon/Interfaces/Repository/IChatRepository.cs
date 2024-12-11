using BlackjackCommon.ViewModels;

namespace BlackjackCommon.Interfaces.Repository
{
	public interface IChatRepository
	{
		List<MessageListModel> RetrieveMessageList(int user_id);
		List<MessageListModel> RetrievePrivateMessages(int user_id, int other_user_id);
		void SaveChatMessage(int user_id, int receiver_id, string group_id, string message);
	}
}
