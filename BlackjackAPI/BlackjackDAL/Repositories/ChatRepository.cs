using BlackjackCommon.Entities.Message;
using BlackjackCommon.Interfaces.Repository;

namespace BlackjackDAL.Repositories
{
	public class ChatRepository : IChatRepository
	{
		private readonly DBConnection _DBConnection = new();

		public List<Message> RetrieveMessageList(int user_id)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);
					
					if (user == null)
					{
						throw new Exception($"User with ID {user_id} not found.");
					}

					// subquery to get the last message exchanged between two users.
					var lastMessages = context.Message
						.Where(m => m.MessageSender == user_id || m.MessageReceiver == user_id)
						.GroupBy(m => new {
							User1 = Math.Max(m.MessageSender, (int)m.MessageReceiver),
							User2 = Math.Min(m.MessageSender, (int)m.MessageReceiver)
						})
						.Select(g => new
						{
							User1 = g.Key.User1,
							User2 = g.Key.User2,
							LatestMessageID = g.Max(m => m.MessageId)
						});

					// main query to get details for said message.
					var messages = from m in context.Message
						join uSender in context.User on m.MessageSender equals uSender.user_id
						join uReceiver in context.User on m.MessageReceiver equals uReceiver.user_id
						join lastMsg in lastMessages on new
						{
						   User1 = Math.Max(m.MessageSender, (int)m.MessageReceiver),
						   User2 = Math.Min(m.MessageSender, (int)m.MessageReceiver)
						} equals new
						{
						   User1 = lastMsg.User1,
						   User2 = lastMsg.User2
						}
						where m.MessageId == lastMsg.LatestMessageID
						orderby m.MessageId descending
						select new Message
						{
						   MessageId = m.MessageId,
						   MessageSender = m.MessageSender,
						   MessageReceiver = m.MessageReceiver,
						   MessageGroup = null,
						   MessageContent = m.MessageDeleted ? "This message has been deleted." : m.MessageContent,
						   MessageDateTime = m.MessageDateTime,
						   MessageDeleted = m.MessageDeleted,
						   SenderUserName = uSender.user_name,
						   ReceiverUserName = uReceiver.user_name
						};

					return messages.ToList();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}

		public List<Message> RetrievePrivateMessages(int user_id, int other_user_id)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var user = context.User.SingleOrDefault(u => u.user_id == user_id);
					
					if (user == null)
					{
						throw new Exception($"User with ID {user_id} not found.");
					}

					var messages = from m in context.Message
						join uSender in context.User on m.MessageSender equals uSender.user_id
						join uReceiver in context.User on m.MessageReceiver equals uReceiver.user_id
						where (m.MessageSender == user_id && m.MessageReceiver == other_user_id)
							 || (m.MessageSender == other_user_id && m.MessageReceiver == user_id)
						orderby m.MessageId descending 
						select new Message
						{
						   MessageId = m.MessageId,
						   MessageSender = m.MessageSender,
						   MessageReceiver = m.MessageReceiver,
						   MessageGroup = null,
						   MessageContent = m.MessageDeleted ? "This message has been deleted." : m.MessageContent,
						   MessageDateTime = m.MessageDateTime,
						   MessageDeleted = m.MessageDeleted,
						   SenderUserName = uSender.user_name,
						   ReceiverUserName = uReceiver.user_name
						};

					return messages.ToList();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
				throw;
			}
		}



		public void SaveChatMessage()
		{
			
		}


	}
}
