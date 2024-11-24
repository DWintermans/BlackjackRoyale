using BlackjackCommon.Entities.Message;
using BlackjackCommon.Entities.User;
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
						.Where(m => m.message_sender == user_id || m.message_receiver == user_id)
						.GroupBy(m => new {
							User1 = Math.Max(m.message_sender, (int)m.message_receiver),
							User2 = Math.Min(m.message_sender, (int)m.message_receiver)
						})
						.Select(g => new
						{
							User1 = g.Key.User1,
							User2 = g.Key.User2,
							LatestMessageID = g.Max(m => m.message_id)
						});

					// main query to get details for said message.
					var messages = from m in context.Message
						join uSender in context.User on m.message_sender equals uSender.user_id
						join uReceiver in context.User on m.message_receiver equals uReceiver.user_id
						join lastMsg in lastMessages on new
						{
						   User1 = Math.Max(m.message_sender, (int)m.message_receiver),
						   User2 = Math.Min(m.message_sender, (int)m.message_receiver)
						} equals new
						{
						   User1 = lastMsg.User1,
						   User2 = lastMsg.User2
						}
						where m.message_id == lastMsg.LatestMessageID
						orderby m.message_id descending
						select new Message
						{
						   message_id = m.message_id,
						   message_sender = m.message_sender,
						   message_receiver = m.message_receiver,
						   message_group = null,
						   message_content = m.message_deleted ? "This message has been deleted." : m.message_content,
						   message_datetime = m.message_datetime,
						   message_deleted = m.message_deleted,
						   sender_username = uSender.user_name,
						   receiver_username = uReceiver.user_name
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
						join uSender in context.User on m.message_sender equals uSender.user_id
						join uReceiver in context.User on m.message_receiver equals uReceiver.user_id
						where (m.message_sender == user_id && m.message_receiver == other_user_id)
							 || (m.message_sender == other_user_id && m.message_receiver == user_id)
						orderby m.message_id descending 
						select new Message
						{
						   message_id = m.message_id,
						   message_sender = m.message_sender,
						   message_receiver = m.message_receiver,
						   message_group = null,
						   message_content = m.message_deleted ? "This message has been deleted." : m.message_content,
						   message_datetime = m.message_datetime,
						   message_deleted = m.message_deleted,
						   sender_username = uSender.user_name,
						   receiver_username = uReceiver.user_name
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



		public void SaveChatMessage(int user_id, int receiver_id, string group_id, string message)
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var newMessage = new Message
					{
						message_sender = user_id,
						message_receiver = receiver_id,
						message_group = group_id,
						message_content = message,
						message_datetime= DateTime.Now,
					};

					context.Message.Add(newMessage);

					context.SaveChanges();

				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}
		}

	}
}
