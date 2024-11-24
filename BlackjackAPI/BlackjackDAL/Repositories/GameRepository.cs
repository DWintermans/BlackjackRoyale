using BlackjackCommon.Entities.Friend;
using BlackjackCommon.Entities.History;
using BlackjackCommon.Interfaces.Repository;

namespace BlackjackDAL.Repositories
{
	public class GameRepository : IGameRepository
	{
		private readonly DBConnection _DBConnection = new();

		public void SaveEvent(int user_id, string group_id, string action, string result, string payload, int round_number) 
		{
			try
			{
				using (var context = new AppDbContext(_DBConnection.ConnectionString()))
				{
					var parsedAction = Enum.Parse<HistoryAction>(action);
					var parsedResult = string.IsNullOrEmpty(result) ? (HistoryResult?)null : Enum.Parse<HistoryResult>(result);

					var historyEntry = new History
					{
						history_group_id = group_id,
						history_user_id = user_id,
						history_action = parsedAction,
						history_result = parsedResult,
						history_payload = payload,
						history_round_number = round_number == 0 ? 1 : round_number, //edge case for betting in a new game
						history_datetime = DateTime.Now 
					};

					context.History.Add(historyEntry);

					context.SaveChanges();
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred while saving the event: {ex.Message}");
				throw;
			}
		}

	}
}
