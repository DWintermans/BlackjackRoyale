using BlackjackCommon.Entities.Message;

namespace BlackjackCommon.Interfaces.Repository
{
	public interface IChatRepository
	{
		List<Message> RetrieveMessageList(int user_id);
		List<Message> RetrievePrivateMessages(int user_id, int other_user_id);
		void SaveChatMessage(int user_id, int receiver_id, string group_id, string message);
	}
}
