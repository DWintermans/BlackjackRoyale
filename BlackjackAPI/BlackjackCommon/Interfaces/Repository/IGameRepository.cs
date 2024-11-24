namespace BlackjackCommon.Interfaces.Repository
{
	public interface IGameRepository
	{
		void SaveEvent(int user_id, string group_id, string action, string result, string payload, int round_number);
	}
}
