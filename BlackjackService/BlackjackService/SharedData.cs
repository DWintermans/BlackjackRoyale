namespace BlackjackService
{
	public static class SharedData
	{
		public static Dictionary<string, string> userIDToCliendIdMap = new Dictionary<string, string>(); //group_id client_id(uuid)
		public static Dictionary<string, List<int>> groupMembers = new Dictionary<string, List<int>>(); //group_id, user_id
		public static Dictionary<int, List<string>> playerHands = new Dictionary<int, List<string>>(); //user_id cards
		public static Dictionary<string, List<string>> groupDeck = new Dictionary<string, List<string>>(); //group_id, cards
		public static Dictionary<string, List<int>> waitingRoom = new Dictionary<string, List<int>>(); //group_id, user_id


	}
}
